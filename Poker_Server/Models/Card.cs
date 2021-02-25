using System;
using System.Collections.Generic;
using System.Linq;
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
			string carta = "U0001F0";
			return "\\" + carta + Cards.pals[pal] + Cards.valors[valor];
		}
	}
}