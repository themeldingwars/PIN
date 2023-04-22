using System;

namespace GameServer.Test.GSS;

public static class ArcCompletionHistoryUpdate
{
    public static Packets.GSS.ArcCompletionHistoryUpdate GetKnownWorking()
    {
        var ret = new Packets.GSS.ArcCompletionHistoryUpdate
                  {
                      Entries = new[]
                                {
                                    new Packets.GSS.ArcCompletionHistoryUpdate.Entry(0x25000000u, 0x01000000u, 0x00000000u, 0x00000000u), new Packets.GSS.ArcCompletionHistoryUpdate.Entry(0x29000000u, 0x01000000u, 0x01000000u, 0x6a5d5c55u),
                                    new Packets.GSS.ArcCompletionHistoryUpdate.Entry(0x43000000u, 0x01000000u, 0x01000000u, 0x00d93754u), new Packets.GSS.ArcCompletionHistoryUpdate.Entry(0x44000000u, 0x02000000u, 0x02000000u, 0xd1e32154u),
                                    new Packets.GSS.ArcCompletionHistoryUpdate.Entry(0x47000000u, 0x04000000u, 0x01000000u, 0xb1fdc654u), new Packets.GSS.ArcCompletionHistoryUpdate.Entry(0x49000000u, 0x01000000u, 0x01000000u, 0xf903c754u),
                                    new Packets.GSS.ArcCompletionHistoryUpdate.Entry(0x4b000000u, 0x01000000u, 0x01000000u, 0xd48b0c55u), new Packets.GSS.ArcCompletionHistoryUpdate.Entry(0x4c000000u, 0x01000000u, 0x01000000u, 0x2adf2154u),
                                    new Packets.GSS.ArcCompletionHistoryUpdate.Entry(0x4d000000u, 0x01000000u, 0x01000000u, 0x12e12154u), new Packets.GSS.ArcCompletionHistoryUpdate.Entry(0x5a000000u, 0x04000000u, 0x04000000u, 0x91d62154u),
                                    new Packets.GSS.ArcCompletionHistoryUpdate.Entry(0x5c000000u, 0x01000000u, 0x01000000u, 0x5de8dc53u), new Packets.GSS.ArcCompletionHistoryUpdate.Entry(0x5d000000u, 0x01000000u, 0x01000000u, 0x77da2154u),
                                    new Packets.GSS.ArcCompletionHistoryUpdate.Entry(0x5e000000u, 0x02000000u, 0x02000000u, 0x18d02154u), new Packets.GSS.ArcCompletionHistoryUpdate.Entry(0x61000000u, 0x02000000u, 0x02000000u, 0xb367c953u),
                                    new Packets.GSS.ArcCompletionHistoryUpdate.Entry(0x65000000u, 0x02000000u, 0x02000000u, 0xd462a655u), new Packets.GSS.ArcCompletionHistoryUpdate.Entry(0x68000000u, 0x01000000u, 0x01000000u, 0xb9dd2154u),
                                    new Packets.GSS.ArcCompletionHistoryUpdate.Entry(0xd0000000u, 0x02000000u, 0x02000000u, 0x1cdd1255u), new Packets.GSS.ArcCompletionHistoryUpdate.Entry(0xde000000u, 0x01000000u, 0x01000000u, 0x41c12154u),
                                    new Packets.GSS.ArcCompletionHistoryUpdate.Entry(0xe6000000u, 0x01000000u, 0x01000000u, 0x67eedc53u), new Packets.GSS.ArcCompletionHistoryUpdate.Entry(0xea000000u, 0x02000000u, 0x02000000u, 0x4bca2154u),
                                    new Packets.GSS.ArcCompletionHistoryUpdate.Entry(0xeb000000u, 0x01000000u, 0x01000000u, 0x2cdb3754u), new Packets.GSS.ArcCompletionHistoryUpdate.Entry(0xed000000u, 0x02000000u, 0x02000000u, 0xfac72154u),
                                    new Packets.GSS.ArcCompletionHistoryUpdate.Entry(0xf1000000u, 0x01000000u, 0x01000000u, 0x0bce2154u), new Packets.GSS.ArcCompletionHistoryUpdate.Entry(0xf2000000u, 0x01000000u, 0x01000000u, 0xeac32154u),
                                    new Packets.GSS.ArcCompletionHistoryUpdate.Entry(0xf4000000u, 0x01000000u, 0x01000000u, 0xe6edcb53u), new Packets.GSS.ArcCompletionHistoryUpdate.Entry(0xf5000000u, 0x01000000u, 0x01000000u, 0xf964a655u),
                                    new Packets.GSS.ArcCompletionHistoryUpdate.Entry(0xfa000000u, 0x01000000u, 0x01000000u, 0x04eacb53u), new Packets.GSS.ArcCompletionHistoryUpdate.Entry(0xfb000000u, 0x01000000u, 0x01000000u, 0xe1ebcb53u),
                                    new Packets.GSS.ArcCompletionHistoryUpdate.Entry(0xfc000000u, 0x01000000u, 0x01000000u, 0x8bf2cb53u), new Packets.GSS.ArcCompletionHistoryUpdate.Entry(0xfe000000u, 0x01000000u, 0x01000000u, 0xd7d22a55u),
                                    new Packets.GSS.ArcCompletionHistoryUpdate.Entry(0x00010000u, 0x02000000u, 0x01000000u, 0x9ed72a55u), new Packets.GSS.ArcCompletionHistoryUpdate.Entry(0x03010000u, 0x01000000u, 0x01000000u, 0x43ad3954u),
                                    new Packets.GSS.ArcCompletionHistoryUpdate.Entry(0x04010000u, 0x01000000u, 0x01000000u, 0x10e33754u), new Packets.GSS.ArcCompletionHistoryUpdate.Entry(0x05010000u, 0x01000000u, 0x01000000u, 0x58753a54u),
                                    new Packets.GSS.ArcCompletionHistoryUpdate.Entry(0x06010000u, 0x02000000u, 0x01000000u, 0xa6322354u), new Packets.GSS.ArcCompletionHistoryUpdate.Entry(0x07010000u, 0x01000000u, 0x01000000u, 0xb7dd3754u),
                                    new Packets.GSS.ArcCompletionHistoryUpdate.Entry(0x08010000u, 0x01000000u, 0x01000000u, 0x33df3754u), new Packets.GSS.ArcCompletionHistoryUpdate.Entry(0x09010000u, 0x01000000u, 0x01000000u, 0xc1e13754u),
                                    new Packets.GSS.ArcCompletionHistoryUpdate.Entry(0x0a010000u, 0x01000000u, 0x01000000u, 0x02ee2154u), new Packets.GSS.ArcCompletionHistoryUpdate.Entry(0x0b010000u, 0x02000000u, 0x01000000u, 0x385a5c55u),
                                    new Packets.GSS.ArcCompletionHistoryUpdate.Entry(0x11010000u, 0x01000000u, 0x01000000u, 0x9132a955u), new Packets.GSS.ArcCompletionHistoryUpdate.Entry(0x16010000u, 0x02000000u, 0x02000000u, 0xf1ec2154u),
                                    new Packets.GSS.ArcCompletionHistoryUpdate.Entry(0x1d010000u, 0x01000000u, 0x01000000u, 0xe1efcb53u), new Packets.GSS.ArcCompletionHistoryUpdate.Entry(0x1e010000u, 0x01000000u, 0x01000000u, 0xe58ec953u),
                                    new Packets.GSS.ArcCompletionHistoryUpdate.Entry(0x25010000u, 0x01000000u, 0x00000000u, 0x00000000u), new Packets.GSS.ArcCompletionHistoryUpdate.Entry(0x26010000u, 0x01000000u, 0x01000000u, 0x74e0d155u),
                                    new Packets.GSS.ArcCompletionHistoryUpdate.Entry(0x28010000u, 0x01000000u, 0x01000000u, 0x317ce055u), new Packets.GSS.ArcCompletionHistoryUpdate.Entry(0x32010000u, 0x01000000u, 0x01000000u, 0x5c99bf55u),
                                    new Packets.GSS.ArcCompletionHistoryUpdate.Entry(0x34010000u, 0x01000000u, 0x01000000u, 0x5c5fd255u), new Packets.GSS.ArcCompletionHistoryUpdate.Entry(0x36010000u, 0x01000000u, 0x01000000u, 0xe34fd255u),
                                    new Packets.GSS.ArcCompletionHistoryUpdate.Entry(0x37010000u, 0x01000000u, 0x01000000u, 0x88dbd155u), new Packets.GSS.ArcCompletionHistoryUpdate.Entry(0x39010000u, 0x02000000u, 0x02000000u, 0x699bbf55u),
                                    new Packets.GSS.ArcCompletionHistoryUpdate.Entry(0x3b010000u, 0x01000000u, 0x01000000u, 0x2553d455u), new Packets.GSS.ArcCompletionHistoryUpdate.Entry(0x3c010000u, 0x01000000u, 0x01000000u, 0xaa57c355u),
                                    new Packets.GSS.ArcCompletionHistoryUpdate.Entry(0x3d010000u, 0x01000000u, 0x01000000u, 0x59523256u), new Packets.GSS.ArcCompletionHistoryUpdate.Entry(0x42010000u, 0x01000000u, 0x01000000u, 0x8c5bc355u),
                                    new Packets.GSS.ArcCompletionHistoryUpdate.Entry(0x43010000u, 0x01000000u, 0x01000000u, 0x827d0c55u), new Packets.GSS.ArcCompletionHistoryUpdate.Entry(0x44010000u, 0x01000000u, 0x00000000u, 0x00000000u),
                                    new Packets.GSS.ArcCompletionHistoryUpdate.Entry(0x48010000u, 0x01000000u, 0x01000000u, 0xe6850c55u), new Packets.GSS.ArcCompletionHistoryUpdate.Entry(0x49010000u, 0x01000000u, 0x01000000u, 0x03810c55u),
                                    new Packets.GSS.ArcCompletionHistoryUpdate.Entry(0x4a010000u, 0x01000000u, 0x01000000u, 0xdb01c155u), new Packets.GSS.ArcCompletionHistoryUpdate.Entry(0x4f010000u, 0x01000000u, 0x01000000u, 0x9055d255u),
                                    new Packets.GSS.ArcCompletionHistoryUpdate.Entry(0x50010000u, 0x01000000u, 0x01000000u, 0x8f53d255u), new Packets.GSS.ArcCompletionHistoryUpdate.Entry(0x51010000u, 0x01000000u, 0x01000000u, 0x5ae2d155u),
                                    new Packets.GSS.ArcCompletionHistoryUpdate.Entry(0x52010000u, 0x01000000u, 0x01000000u, 0x4705c155u), new Packets.GSS.ArcCompletionHistoryUpdate.Entry(0x53010000u, 0x02000000u, 0x02000000u, 0x3897bf55u),
                                    new Packets.GSS.ArcCompletionHistoryUpdate.Entry(0x54010000u, 0x02000000u, 0x01000000u, 0x0a65d355u), new Packets.GSS.ArcCompletionHistoryUpdate.Entry(0x58010000u, 0x01000000u, 0x01000000u, 0xce80e055u),
                                    new Packets.GSS.ArcCompletionHistoryUpdate.Entry(0x59010000u, 0x01000000u, 0x01000000u, 0xe1380056u), new Packets.GSS.ArcCompletionHistoryUpdate.Entry(0x5a010000u, 0x01000000u, 0x01000000u, 0x2b502556u),
                                    new Packets.GSS.ArcCompletionHistoryUpdate.Entry(0x5b010000u, 0x01000000u, 0x01000000u, 0x21c66556u), new Packets.GSS.ArcCompletionHistoryUpdate.Entry(0x5c010000u, 0x01000000u, 0x01000000u, 0x18532556u),
                                    new Packets.GSS.ArcCompletionHistoryUpdate.Entry(0x5d010000u, 0x01000000u, 0x01000000u, 0x0612d555u), new Packets.GSS.ArcCompletionHistoryUpdate.Entry(0x5e010000u, 0x02000000u, 0x01000000u, 0x005bd455u),
                                    new Packets.GSS.ArcCompletionHistoryUpdate.Entry(0x60010000u, 0x01000000u, 0x01000000u, 0xfb21de55u), new Packets.GSS.ArcCompletionHistoryUpdate.Entry(0x61010000u, 0x01000000u, 0x01000000u, 0xb33c0056u),
                                    new Packets.GSS.ArcCompletionHistoryUpdate.Entry(0x62010000u, 0x02000000u, 0x02000000u, 0x73582556u), new Packets.GSS.ArcCompletionHistoryUpdate.Entry(0x65010000u, 0x01000000u, 0x01000000u, 0x381cde55u),
                                    new Packets.GSS.ArcCompletionHistoryUpdate.Entry(0x67010000u, 0x02000000u, 0x00000000u, 0x00000000u), new Packets.GSS.ArcCompletionHistoryUpdate.Entry(0x68010000u, 0x01000000u, 0x01000000u, 0x46bc6556u),
                                    new Packets.GSS.ArcCompletionHistoryUpdate.Entry(0x69010000u, 0x01000000u, 0x01000000u, 0x37be6556u), new Packets.GSS.ArcCompletionHistoryUpdate.Entry(0x6b010000u, 0x01000000u, 0x01000000u, 0x091fde55u),
                                    new Packets.GSS.ArcCompletionHistoryUpdate.Entry(0x6c010000u, 0x01000000u, 0x01000000u, 0xfb78e055u), new Packets.GSS.ArcCompletionHistoryUpdate.Entry(0x6d010000u, 0x01000000u, 0x01000000u, 0x5c7de055u),
                                    new Packets.GSS.ArcCompletionHistoryUpdate.Entry(0x70010000u, 0x02000000u, 0x02000000u, 0x2f5e2556u), new Packets.GSS.ArcCompletionHistoryUpdate.Entry(0x71010000u, 0x02000000u, 0x02000000u, 0x10552556u),
                                    new Packets.GSS.ArcCompletionHistoryUpdate.Entry(0x74010000u, 0x02000000u, 0x02000000u, 0xf891bf55u), new Packets.GSS.ArcCompletionHistoryUpdate.Entry(0x75010000u, 0x01000000u, 0x01000000u, 0xa628de55u),
                                    new Packets.GSS.ArcCompletionHistoryUpdate.Entry(0x77010000u, 0x01000000u, 0x01000000u, 0x737fe055u), new Packets.GSS.ArcCompletionHistoryUpdate.Entry(0x78010000u, 0x02000000u, 0x02000000u, 0x27622556u),
                                    new Packets.GSS.ArcCompletionHistoryUpdate.Entry(0x79010000u, 0x04000000u, 0x01000000u, 0x8c67d455u), new Packets.GSS.ArcCompletionHistoryUpdate.Entry(0x80010000u, 0x01000000u, 0x01000000u, 0x14b8dc53u),
                                    new Packets.GSS.ArcCompletionHistoryUpdate.Entry(0x85010000u, 0x01000000u, 0x01000000u, 0xe3bef053u), new Packets.GSS.ArcCompletionHistoryUpdate.Entry(0x89010000u, 0x01000000u, 0x01000000u, 0xd8b8f053u),
                                    new Packets.GSS.ArcCompletionHistoryUpdate.Entry(0x8b010000u, 0x01000000u, 0x01000000u, 0xfcbbf053u), new Packets.GSS.ArcCompletionHistoryUpdate.Entry(0x8c010000u, 0x04000000u, 0x01000000u, 0xbc69f253u),
                                    new Packets.GSS.ArcCompletionHistoryUpdate.Entry(0x8d010000u, 0x01000000u, 0x01000000u, 0x0c6df253u), new Packets.GSS.ArcCompletionHistoryUpdate.Entry(0x95010000u, 0x01000000u, 0x00000000u, 0x00000000u),
                                    new Packets.GSS.ArcCompletionHistoryUpdate.Entry(0xca010000u, 0x02000000u, 0x00000000u, 0x00000000u), new Packets.GSS.ArcCompletionHistoryUpdate.Entry(0xd0010000u, 0x01000000u, 0x01000000u, 0xa771f253u),
                                    new Packets.GSS.ArcCompletionHistoryUpdate.Entry(0x4a030000u, 0x1f000000u, 0x01000000u, 0x65b2a656u), new Packets.GSS.ArcCompletionHistoryUpdate.Entry(0x4b030000u, 0x02000000u, 0x01000000u, 0x1f32aa56u),
                                    new Packets.GSS.ArcCompletionHistoryUpdate.Entry(0x4c030000u, 0x01000000u, 0x01000000u, 0x3b36ab56u), new Packets.GSS.ArcCompletionHistoryUpdate.Entry(0x4e030000u, 0x04000000u, 0x01000000u, 0xfa29aa56u),
                                    new Packets.GSS.ArcCompletionHistoryUpdate.Entry(0x4f030000u, 0x02000000u, 0x01000000u, 0xe733aa56u), new Packets.GSS.ArcCompletionHistoryUpdate.Entry(0x50030000u, 0x02000000u, 0x01000000u, 0x3439aa56u),
                                    new Packets.GSS.ArcCompletionHistoryUpdate.Entry(0x51030000u, 0x03000000u, 0x01000000u, 0x1c30ab56u), new Packets.GSS.ArcCompletionHistoryUpdate.Entry(0x56030000u, 0x29000000u, 0x01000000u, 0x0735ab56u),
                                    new Packets.GSS.ArcCompletionHistoryUpdate.Entry(0xb5040000u, 0x01000000u, 0x01000000u, 0x61d6c156u), new Packets.GSS.ArcCompletionHistoryUpdate.Entry(0xb6040000u, 0x01000000u, 0x01000000u, 0xb0d2c156u),
                                    new Packets.GSS.ArcCompletionHistoryUpdate.Entry(0xb7040000u, 0x10000000u, 0x01000000u, 0x1ed2c356u), new Packets.GSS.ArcCompletionHistoryUpdate.Entry(0xbd040000u, 0x02000000u, 0x00000000u, 0x00000000u),
                                    new Packets.GSS.ArcCompletionHistoryUpdate.Entry(0xe5040000u, 0x01000000u, 0x01000000u, 0x94903457u), new Packets.GSS.ArcCompletionHistoryUpdate.Entry(0xe6040000u, 0x01000000u, 0x01000000u, 0xd68b3457u),
                                    new Packets.GSS.ArcCompletionHistoryUpdate.Entry(0xe7040000u, 0x01000000u, 0x01000000u, 0x1d942057u), new Packets.GSS.ArcCompletionHistoryUpdate.Entry(0xe8040000u, 0x02000000u, 0x02000000u, 0xb59e3457u),
                                    new Packets.GSS.ArcCompletionHistoryUpdate.Entry(0xe9040000u, 0x01000000u, 0x01000000u, 0xfb863457u), new Packets.GSS.ArcCompletionHistoryUpdate.Entry(0xea040000u, 0x01000000u, 0x01000000u, 0xbb1d2157u),
                                    new Packets.GSS.ArcCompletionHistoryUpdate.Entry(0xeb040000u, 0x01000000u, 0x01000000u, 0x98893457u), new Packets.GSS.ArcCompletionHistoryUpdate.Entry(0xec040000u, 0x01000000u, 0x01000000u, 0x7b8d3457u),
                                    new Packets.GSS.ArcCompletionHistoryUpdate.Entry(0xed040000u, 0x01000000u, 0x01000000u, 0x738f2057u), new Packets.GSS.ArcCompletionHistoryUpdate.Entry(0xee040000u, 0x01000000u, 0x01000000u, 0x8ca53457u),
                                    new Packets.GSS.ArcCompletionHistoryUpdate.Entry(0xef040000u, 0x02000000u, 0x02000000u, 0x93bf3057u), new Packets.GSS.ArcCompletionHistoryUpdate.Entry(0xf6040000u, 0x02000000u, 0x01000000u, 0xdacf2c57u),
                                    new Packets.GSS.ArcCompletionHistoryUpdate.Entry(0xf7040000u, 0x01000000u, 0x01000000u, 0xcca12f57u), new Packets.GSS.ArcCompletionHistoryUpdate.Entry(0xf8040000u, 0x01000000u, 0x01000000u, 0xf5a22f57u),
                                    new Packets.GSS.ArcCompletionHistoryUpdate.Entry(0xf9040000u, 0x01000000u, 0x01000000u, 0x0aa52f57u), new Packets.GSS.ArcCompletionHistoryUpdate.Entry(0xfa040000u, 0x01000000u, 0x01000000u, 0x93a62f57u),
                                    new Packets.GSS.ArcCompletionHistoryUpdate.Entry(0xfb040000u, 0x02000000u, 0x02000000u, 0x28a02f57u), new Packets.GSS.ArcCompletionHistoryUpdate.Entry(0xfc040000u, 0x01000000u, 0x01000000u, 0x5ea33457u),
                                    new Packets.GSS.ArcCompletionHistoryUpdate.Entry(0xfd040000u, 0x01000000u, 0x01000000u, 0x81a63457u), new Packets.GSS.ArcCompletionHistoryUpdate.Entry(0xfe040000u, 0x01000000u, 0x01000000u, 0x5ca73457u)
                                }
                  };

        return ret;
    }

    public static Packets.GSS.ArcCompletionHistoryUpdate GetEmpty()
    {
        var ret = new Packets.GSS.ArcCompletionHistoryUpdate { Entries = Array.Empty<Packets.GSS.ArcCompletionHistoryUpdate.Entry>() };

        return ret;
    }
}