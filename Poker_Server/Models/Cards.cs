using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Web;

namespace Poker_Server.Models
{
	public static class Cards
	{
		public static string Cover       = "\U0001F0A0";

		public static string AceSpades   = "\U0001F0A1";
		public static string TwoSpades   = "\U0001F0A2";
		public static string ThreeSpades = "\U0001F0A3";
		public static string FourSpades  = "\U0001F0A4";
		public static string FiveSpades  = "\U0001F0A5";
		public static string SixSpades   = "\U0001F0A6";
		public static string SevenSpades = "\U0001F0A7";
		public static string EightSpades = "\U0001F0A8";
		public static string NineSpades  = "\U0001F0A9";
		public static string TenSpades   = "\U0001F0AA";
		public static string JackSpades  = "\U0001F0AB";
		public static string QueenSpades = "\U0001F0AD";
		public static string KingSpades  = "\U0001F0AE";

		public static string AceHearts   = "\U0001F0B1";
		public static string TwoHearts   = "\U0001F0B2";
		public static string ThreeHearts = "\U0001F0B3";
		public static string FourHearts  = "\U0001F0B4";
		public static string FiveHearts  = "\U0001F0B5";
		public static string SixHearts   = "\U0001F0B6";
		public static string SevenHearts = "\U0001F0B7";
		public static string EightHearts = "\U0001F0B8";
		public static string NineHearts  = "\U0001F0B9";
		public static string TenHearts   = "\U0001F0BA";
		public static string JackHearts  = "\U0001F0BB";
		public static string QueenHearts = "\U0001F0BD";
		public static string KingHearts  = "\U0001F0BE";

		public static string AceDiamonds   = "\U0001F0C1";
		public static string TwoDiamonds   = "\U0001F0C2";
		public static string ThreeDiamonds = "\U0001F0C3";
		public static string FourDiamonds  = "\U0001F0C4";
		public static string FiveDiamonds  = "\U0001F0C5";
		public static string SixDiamonds   = "\U0001F0C6";
		public static string SevenDiamonds = "\U0001F0C7";
		public static string EightDiamonds = "\U0001F0C8";
		public static string NineDiamonds  = "\U0001F0C9";
		public static string TenDiamonds   = "\U0001F0CA";
		public static string JackDiamonds  = "\U0001F0CB";
		public static string QueenDiamonds = "\U0001F0CD";
		public static string KingDiamonds  = "\U0001F0CE";

		public static string AceClubs   = "\U0001F0D1";
		public static string TwoClubs   = "\U0001F0D2";
		public static string ThreeClubs = "\U0001F0D3";
		public static string FourClubs  = "\U0001F0D4";
		public static string FiveClubs  = "\U0001F0D5";
		public static string SixClubs   = "\U0001F0D6";
		public static string SevenClubs = "\U0001F0D7";
		public static string EightClubs = "\U0001F0D8";
		public static string NineClubs  = "\U0001F0D9";
		public static string TenClubs   = "\U0001F0DA";
		public static string JackClubs  = "\U0001F0DB";
		public static string QueenClubs = "\U0001F0DD";
		public static string KingClubs  = "\U0001F0DE";

		public static Dictionary<string, string> pals = new Dictionary<string, string> {
			{"Spades", "A" },
			{"Hearts", "B" },
			{"Diamonds", "C" },
			{"Clubs", "D" }
		};

		public static Dictionary<int, string> valors = new Dictionary<int, string> {
			{1, "2" },
			{2, "3" },
			{3, "4" },
			{4, "5" },
			{5, "6" },
			{6, "7" },
			{7, "8" },
			{8, "9" },
			{9, "A" },
			{10, "B" },
			{11, "C" },
			{12, "D" },
			{13, "E" },
			{14, "1" }
		};

		public static string[] RedCards = { AceDiamonds,   AceHearts,
											TwoDiamonds,   TwoHearts,
											ThreeDiamonds, ThreeHearts,
											FourDiamonds,  FourHearts,
											FiveDiamonds,  FiveHearts,
											SixDiamonds,   SixHearts,
											SevenDiamonds, SevenHearts,
											EightDiamonds, EightHearts,
											NineDiamonds,  NineHearts,
											TenDiamonds,   TenHearts,
											JackDiamonds,  JackHearts,
											QueenDiamonds, QueenHearts,
											KingDiamonds,  KingHearts  };

		public static string[] BlackCards = { AceClubs   ,   AceSpades   ,
											  TwoClubs   ,   TwoSpades   ,
											  ThreeClubs ,   ThreeSpades ,
											  FourClubs  ,   FourSpades  ,
											  FiveClubs  ,   FiveSpades  ,
										  	  SixClubs   ,   SixSpades   ,
											  SevenClubs ,   SevenSpades ,
											  EightClubs ,   EightSpades ,
											  NineClubs  ,   NineSpades  ,
											  TenClubs   ,   TenSpades   ,
											  JackClubs  ,   JackSpades  ,
											  QueenClubs ,   QueenSpades ,
											  KingClubs  ,   KingSpades  };

		public static List<Card> GenerarBaralla()
		{
			//List<string> baralla = new List<string>();
			List<Card> baralla = new List<Card>(52);
			
			int n = 0;
			for (int i = 0; i < 13; i++)
			{
				baralla.Add(new Card("Spades", i + 1));
			}
			for (int i = 0; i < 13; i++)
			{
				baralla.Add(new Card("Hearts", i + 1));
			}
			for (int i = 0; i < 13; i++)
			{
				baralla.Add(new Card("Diamonds", i + 1));
			}
			for (int i = 0; i < 13; i++)
			{
				baralla.Add(new Card("Clubs", i + 1));
			}
			Console.WriteLine(baralla[0].ToString());


			//baralla.AddRange(BlackCards);
			//baralla.AddRange(RedCards);

			/*for (int i = 0; i < baralla.Count; i++)
			{
				//int k = r.Next(baralla.Count-i);
				int k = random(baralla.Count - i);
				var t = baralla[k];
				baralla[k] = baralla[i];
				baralla[i] = t;
			}
			for (int i = 0; i < baralla.Count; i++)
			{
				//int k = r.Next(baralla.Count-i);
				int k = random(baralla.Count - i);
				var t = baralla[k];
				baralla[k] = baralla[i];
				baralla[i] = t;
			}*/
			return baralla;
		}

		static int random(int maxim) // random més variable que el de la funció Random
		{
			using (RNGCryptoServiceProvider rg = new RNGCryptoServiceProvider())
			{
				byte[] rno = new byte[5];
				rg.GetBytes(rno);
				int randomvalue = BitConverter.ToInt32(rno, 0);
				return Math.Abs(randomvalue) % maxim;
			}
		}
	}
}