using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace Poker_Server.Models
{
	public class Card
	{
		public string pal;
		public int valor;

		public Card(string pal, int valor)
		{
			this.pal = pal;
			this.valor = valor;
		}

		public override string ToString()
		{
			//string pre = System.Text.RegularExpressions.Regex.Unescape("U0001F0");
			Encoding unicode = Encoding.Unicode;
			string test = "\\U0001F0{0}{1}";
			string carta = "0001F0" + Cards.pals[pal] + Cards.valors[valor];
			string s = string.Format(@"\U{0:x4}", carta);
			//string s = string.Format("{0}{1}{2}", carta, Cards.pals[pal], Cards.valors[valor]);
			return s;
			// string.Concat("\\", carta, Cards.pals[pal], Cards.valors[valor])
		}
	}
}