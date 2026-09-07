# Investigation: "Connection Problem" + failed wings deploy on a static glider pad

**Branch investigated:** `arena/01a07d59-pin` @ `b7728a0` (= `origin/master`)
**Reported symptom:** stepping onto a static in-world glider pad that boosts the player
into the air shows the client's **"Connection Problem"** dialog, the wings/glider deploy
animation starts but **fails and does nothing**, while the server keeps running.
**Reported timing:** a regression — it worked before the most recent PRs.

> Scope note. I can't run the proprietary Firefall client in this sandbox, so this is a
> server-side code analysis plus a concrete, testable hypothesis. The exact client
> internal reason a pad launch "tries to play the wings animation then aborts" cannot be
> proven from the server source alone; it needs one in-game confirmation step (below).

---

## 1. Where the pad launch actually happens (server vs. client)

For **players**, movement is *client-authoritative*. A static glider pad at e.g. a
Watchtower is not modelled by GameServer — it is zone geometry. When you walk onto it:

1. The **client** locally simulates the launch impulse and the wings/glider deploy FX.
2. The client reports the resulting airborne pose to the server as a
   `GssCharacterCommand.MovementInput`
   (`UdpHosts/GameServer/Controllers/Character/BaseController.cs`,
   `Systems/MovementRelay/MovementRelay.cs`).
3. The server mirrors the pose into a kinematic physics body (`PhysicsEngine.UpdateEntity`),
   feeds the pose to the fall-damage tracker, and answers the **authoring** client with a
   `ConfirmedPoseUpdate`. It forwards a `CurrentPoseUpdate` (and a `JumpActioned` when the
   pose shows a jump) only to **remote** clients so they can render you as an avatar.

So there is no server feature that "deploys the glider." A server regression can only
reach the pad through **how the server acknowledges/re-broadcasts your launch pose**, and
the most recent PRs did change exactly that code path.

## 2. The one concrete recent change on this path: PR #36 (`273fce4`)

Of the recent PRs (#34 NPC-AI NaN/orientation/ground, #35 mob spawn pose/ground, #36 sprint
flicker / movement-state flag fix, #37 `TargetConeAE`), only **#36** edited the code a pad
launch flows through.

**Before #36** (`MovementRelay.CharacterMovementInput`) the broadcast loop sent *every*
playing client — **including the authoring client itself** — both the `CurrentPoseUpdate`
and, on a jump, a `JumpActioned`:

```csharp
foreach (var remoteClient in _shard.Clients.Values)
{
    if (remoteClient.Status.Equals(IPlayer.PlayerStatus.Playing))
    {
        if (sendJumpActioned)
            remoteClient...SendMessage(new JumpActioned { ShortTime = input.ShortTime }, entityId);
        remoteClient...SendMessage(currentPose, entityId);
    }
}
```

**After #36** the loop now `continue`s past the authoring client:

```csharp
if (remoteClient.SocketId == client.SocketId)
    continue;   // <-- NEW: authoring client no longer gets JumpActioned OR CurrentPoseUpdate
```

This fixed the reported **sprint animation flicker** (re-sending your own
`CurrentPoseUpdate` every tick re-drives a movement state the client is already
predicting). But the fix removed **both** self-echoes, not just `CurrentPoseUpdate`. In a
**solo session** (no second player), the loop now sends the authoring client *nothing* —
the client's only per-tick answer is `ConfirmedPoseUpdate`.

**Leading hypothesis:** a pad launch is reported to the server as a *jump* pose update
(`poseData.TimeSinceLastJump` resets → `sendJumpActioned == true`,
`MovementRelay.cs:38`). Before #36 the client still received a `JumpActioned` echo for its
own launch; after #36 it never does. If the client's launch/wings-deploy pipeline waits on
that jump acknowledgement to commit its airborne state, removing it leaves the client
"expecting a launch that the server never confirms": the wings animation starts then
aborts and does nothing, and the repeated unconfirmed launch action trips the client's
sync watchdog → **"Connection Problem"** — while the server, having only ever echoed a
pose, keeps running fine.

This matches the facts well:
- It is a **regression from a recent PR** (#36).
- It is **player-specific** (only the authoring client) and invisible to the server.
- The server "keeps working" because the pad never touched any server logic.

> The second #36 hunk (`MovementStateContainer.SetMovementFlag` XOR→clear) is almost
> certainly irrelevant here: nothing in the pad path sets those flags to `false`
> server-side; they are only read for collision shape choice and aptitude requirements.

## 3. Recommended confirmation (fast, no deep logs needed)

The cleanest check is a **two-build bisect** of just #36 — you can do it by re-applying the
pre-#36 loop and re-testing on the pad:

1. Build current `master` and reproduce the bug on the pad. ✔
2. Temporarily replace the #36 broadcast-loop hunk in
   `UdpHosts/GameServer/Systems/MovementRelay/MovementRelay.cs` (the
   `if (remoteClient.SocketId == client.SocketId) continue;` lines 92–98) with the old
   loop (no self-skip), rebuild, and step on the pad again.
   - If the pad now works (and only the old sprint flicker returns), #36 is confirmed as
     the regression.

You can reproduce "old behaviour" exactly with:

```bash
git show 273fce4~1:UdpHosts/GameServer/Systems/MovementRelay/MovementRelay.cs
```

## 4. Recommended fix (to apply if #3 confirms)

Keep the real improvement from #36 — do **not** send the authoring client its own
`CurrentPoseUpdate` (that is what caused the sprint flicker). But keep delivering a jump
**acknowledgement** (`JumpActioned`) to the authoring client, because a self jump/launch
still needs to be acked even though the remote *pose* should not be re-applied locally.

Suggested shape for the loop in `CharacterMovementInput`:

```csharp
foreach (var remoteClient in _shard.Clients.Values)
{
    bool isSelf = remoteClient.SocketId == client.SocketId;

    // The authoring client already got the authoritative ConfirmedPoseUpdate; don't
    // re-apply the "remote avatar" CurrentPoseUpdate to your own entity (sprint flicker).
    // But it still needs the JumpActioned acknowledgement to commit a self launch (glider
    // pad / jump), so only skip the pose broadcast, not the jump ack.
    if (!isSelf && remoteClient.Status.Equals(IPlayer.PlayerStatus.Playing))
    {
        remoteClient.NetChannels[ChannelType.UnreliableGss].SendMessage(currentPose, character.EntityId);
    }

    if (sendJumpActioned && remoteClient.Status.Equals(IPlayer.PlayerStatus.Playing))
    {
        remoteClient.NetChannels[ChannelType.UnreliableGss]
            .SendMessage(new JumpActioned { ShortTime = input.ShortTime }, character.EntityId);
    }
}
```

Verify in-game afterwards:
- Stepping on the glider pad → launch + wings deploy play and you stay connected.
- Holding **shift / sprint** while moving → no first-person animation flicker (regression
  check for #36's original fix).
- (Optional) with a second client, watch the other player jump — the remote avatar must
  still get its `JumpActioned` + `CurrentPoseUpdate`.

If this hypothesis is wrong, the fallback is the broader route in §5.

## 5. If the jump-ack hypothesis is wrong

The symptom could also come from the client failing to reconcile the launch *velocity /
movement-state* the server echoes, or a glider-permission gap. In that case the decisive
evidence is a **packet-level capture** of what GameServer sends to the client in the
instant you step on the pad (a Wireshark trace of the UDP GSS traffic, or temporarily
logging in `MovementRelay.CharacterMovementInput`: `sendJumpActioned`,
`input.ShortTime`, `poseData.Velocity`, `posRotState.MovementState`, and
`GroundTimePositiveAirTimeNegative` around the flip to airborne). Because player movement
is client-authoritative, capture the *outbound* `ConfirmedPoseUpdate`/`CurrentPoseUpdate`/
`JumpActioned` bytes for that single frame and compare them to the pre-#36 build. The
difference will point directly at the field the client chokes on.

Other candidates, in rough order of likelihood, for completeness:

| # | Candidate | Why it's probably *not* it |
|---|-----------|---------------------------|
| 1 | **PR #36 self-echo removal** (§2) | ✅ Primary suspect — only recent PR on the movement path |
| 2 | PR #34/#35 NPC NaN / orientation / `FindGround` | These touch `CharacterEntity.EstimateInputVelocity`/physics used mainly by NPC AI; a *static* pad for the player shouldn't run them |
| 3 | PR #37 `TargetConeAE` | Cone targeting; unrelated to a pad launch unless the pad fires an ability chain that newly acquires a target |
| 4 | Client glider permission/profile not granted | Would be a pre-existing gap, not a "recent PR regression" |

---

## Status

- [x] Confirmed the pad/launch is client-simulated and server merely mirrors + acks the pose.
- [x] Isolated the only recent server change on that path (PR #36, `273fce4`).
- [x] Produced a concrete, low-risk recommended fix (§4) and a fast confirmation build (§3).
- [x] The §4 fix is **implemented** in `UdpHosts/GameServer/Systems/MovementRelay/MovementRelay.cs` (committed to `arena/01a07d59-pin`; see the CHANGELOG entry).
- [ ] In-game verification (client-side) — needs you to run the build; server can't confirm it alone. If it does not resolve, do the §3 bisect / §5 packet capture and report back.
