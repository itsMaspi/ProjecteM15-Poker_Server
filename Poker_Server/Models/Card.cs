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
		public string text;

		public Card(string pal, int valor, string text)
		{
			this.pal = pal;
			this.valor = valor;
			this.text = text;
		}

		public override string ToString()
		{
			return text;
		}
	}
}