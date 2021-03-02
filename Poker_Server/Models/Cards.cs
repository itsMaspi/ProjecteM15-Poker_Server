using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace Poker_Server.Models
{
	public static class Cards
	{
		public static string Cover       = "\U0001F0A0";

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

		public static List<Card> GenerarBaralla()
		{
			//List<string> baralla = new List<string>();
			List<Card> baralla = new List<Card>(52);
			
			int n = 0;
			byte b = 161;
			byte[] bytes = Encoding.Unicode.GetBytes(Cover);

			for (int i = 0; i < 13; i++)
			{
				if (i == 11)
				{
					b++;
				}
				bytes[2] = b++;
				baralla.Add(new Card("Spades", i + 1, Encoding.Unicode.GetString(bytes)));
			}
			b += 2;
			for (int i = 0; i < 13; i++)
			{
				if (i == 11)
				{
					b++;
				}
				bytes[2] = b++;
				baralla.Add(new Card("Hearts", i + 1, Encoding.Unicode.GetString(bytes)));
			}
			b += 2;
			for (int i = 0; i < 13; i++)
			{
				if (i == 11)
				{
					b++;
				}
				bytes[2] = b++;
				baralla.Add(new Card("Diamonds", i + 1, Encoding.Unicode.GetString(bytes)));
			}
			b += 2;
			for (int i = 0; i < 13; i++)
			{
				if (i == 11)
				{
					b++;
				}
				bytes[2] = b++;
				baralla.Add(new Card("Clubs", i + 1, Encoding.Unicode.GetString(bytes)));
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