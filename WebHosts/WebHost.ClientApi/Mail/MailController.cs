using Microsoft.AspNetCore.Mvc;
using WebHost.ClientApi.Mail.Models;

namespace WebHost.ClientApi.Mail;

[ApiController]
public class MailController : ControllerBase
{
    [Route("api/v2/characters/{characterId}/mail")]
    [HttpGet]
    public object GetMail(string characterId)
    {
        /* Querystring page=1 is default
         * Collect only x email per page
         */
        if (string.IsNullOrEmpty(characterId))
        {
            return new { };
        }

        var mail = new Models.Mail
                   {
                       Count = 2,
                       Results = new MailMessage[]
                                 {
                                     new()
                                     {
                                         Id = 373321,
                                         Subject = "It's been a crazy day - Free gifts inside",
                                         SenderGuid = 0,
                                         SenderName = "Red 5 Studios",
                                         Body = "Hello,\n\nLast night we released a new patch to Firefall that provided many new updates to the game including; more content, the first story campaign missions, new progression changes, and more. As part of this update, the way gear and equipment is calculated changed requiring us to mark your old gear as un-equippable. We provided temporary gear that would be appropriate for your battleframes, but this gear had constraint values that were scaled too high for the quality of the equipment making it difficult to equip your battleframes.\n\nThe Accord are delivering new crates of equipment to you and should be available via the Calldown section of your inventory. If you are tier 3 or 4, this new gear crate will have two sets of equipment at different qualities so that you can mix and match.\n\nWe also realize that many of you have spent large amounts of time and resources to craft the gear that is now obsolete. As our way of saying thank you for your understanding and patience with these changes during our Beta process, we've attached a few rewards to this message. These include:\n\n   - A 7-day VIP package giving you access to crystite bonuses, additional Marketplace slots, and more Workbenches for crafting.\n\n   - A stack of 5 1-hour 20% resource boosts that you can apply to rebuild your stockpiles.\n\nThe attached items will appear in your inventory after clicking the Redeem All button below.\n\nYou are important to us and we want to thank you for participating in the Firefall Beta and your patience while we work towards making Firefall the best game it can be.\n\nSincerely,\n\nThe Red 5 Tribe",
                                         Unread = true,
                                         MailType = "mail",
                                         CreatedAt = 1391247422,
                                         AttachmentCount = 6,
                                         Attachments = new MailAttachments[]
                                                       {
                                                           new() { ItemSdbId = 86360, ItemId = null, Quantity = 1, Claimed = false },
                                                           new() { ItemSdbId = 81361, ItemId = 9160962539553639933, Quantity = 1, Claimed = false },
                                                           new() { ItemSdbId = 81361, ItemId = 9157318587727971069, Quantity = 1, Claimed = false },
                                                           new() { ItemSdbId = 81361, ItemId = 9177587098021734397, Quantity = 1, Claimed = false },
                                                           new() { ItemSdbId = 81361, ItemId = 9168393176584511997, Quantity = 1, Claimed = false },
                                                           new() { ItemSdbId = 81361, ItemId = 9164439237885112061, Quantity = 1, Claimed = false }
                                                       }
                                     },
                                     new()
                                     {
                                         Id = 3453221,
                                         Subject = "Happy Valentine's Day!",
                                         SenderGuid = 0,
                                         SenderName = "Red 5 Studios",
                                         Body = "Happy Valentine's Day! \r\n\r\nThank you for joining us in New Eden, today! Please accept this gift of 5 candy heart consumables. You can use them to \"show the love\" to other players, giving them a 1 hour boost to XP.\r\n\r\nWith Love,\r\nThe Firefall Dev Team\r\n",
                                         Unread = false,
                                         MailType = "mail",
                                         CreatedAt = 1392395174,
                                         AttachmentCount = 1,
                                         Attachments = new MailAttachments[] { new() { ItemSdbId = 85824, ItemId = null, Quantity = 5, Claimed = false } }
                                     }
                                 }
                   };

        return mail;
    }

    [Route("api/v2/characters/{characterId}/mail/{messageId}/claim_attachments")]
    [HttpGet]
    public void GetMailAttachments()
    {
    }
}