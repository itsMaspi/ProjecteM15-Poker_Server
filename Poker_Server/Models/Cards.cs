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

		public static List<Card> GenerarBaralla()
		{
			//List<string> baralla = new List<string>();
			List<Card> baralla = new List<Card>(52);
			
			int n = 0;
			byte b = 161;
			byte[] bytes = Encoding.Unicode.GetBytes(Cover);

			for (int i = 0; i < 13; i++)
			{
				var valor = i + 1;
				if (i == 11)
				{
					b++;
				}
				bytes[2] = b++;
				if (i == 0)
				{
					valor = 14;
				}
				baralla.Add(new Card("Spades", valor, Encoding.Unicode.GetString(bytes)));
			}
			b += 2;
			for (int i = 0; i < 13; i++)
			{
				var valor = i + 1;
				if (i == 11)
				{
					b++;
				}
				bytes[2] = b++;
				if (i == 0)
				{
					valor = 14;
				}
				baralla.Add(new Card("Hearts", valor, Encoding.Unicode.GetString(bytes)));
			}
			b += 2;
			for (int i = 0; i < 13; i++)
			{
				var valor = i + 1;
				if (i == 11)
				{
					b++;
				}
				bytes[2] = b++;
				if (i == 0)
				{
					valor = 14;
				}
				baralla.Add(new Card("Diamonds", valor, Encoding.Unicode.GetString(bytes)));
			}
			b += 2;
			for (int i = 0; i < 13; i++)
			{
				var valor = i + 1;
				if (i == 11)
				{
					b++;
				}
				bytes[2] = b++;
				if (i == 0)
				{
					valor = 14;
				}
				baralla.Add(new Card("Clubs", valor, Encoding.Unicode.GetString(bytes)));
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