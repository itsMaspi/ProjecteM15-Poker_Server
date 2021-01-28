using Microsoft.Web.WebSockets;
using Poker_Server.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;

namespace Poker_Server.Controllers
{
    public class WebSocketController : ApiController
    {
        public HttpResponseMessage Get(string nom)
        {
            // La crida al websocket serà del tipus   ws://host:port/api/websocket?nom=Pere

            HttpContext.Current.AcceptWebSocketRequest(new SocketHandler(nom)); return new HttpResponseMessage(HttpStatusCode.SwitchingProtocols);
        }

        private class SocketHandler : WebSocketHandler
        {
            #region Command prefixes
            private static readonly string PRE_UsersOnline = "/online ";
            private static readonly string PRE_StartGame = "/start ";
			#endregion

			private static readonly WebSocketCollection Sockets = new WebSocketCollection();

            private readonly string _nom;

            private Random random = new Random();
            private static List<string> Baralla = Cards.GenerarBaralla();

            public SocketHandler(string nom)
            {
                _nom = nom;
            }

            public override void OnOpen()
            {
                // Quan es connecta un nou usuari: cal afegir el SocketHandler a la Collection, notificar a tothom la incorporació i donar-li la benvinguda
                Sockets.Broadcast(_nom + " s'ha connectat.");
                Sockets.Add(this);
                Sockets.Broadcast(PRE_UsersOnline + CountConnectedUsers());
                sendInitialCards();
                //Sockets.Broadcast("/showcard " + Baralla.ElementAt(random.Next(Baralla.Count)));
				if (Sockets.Count >= 2)
				{
                    Sockets.Broadcast("Es podria començar la partida...");
				}
                Send("Benvingut " + _nom + "!");
            }

            public override void OnMessage(string missatge)
            {
                // Quan un usuari envia un missatge, cal que tothom el rebi
                if (missatge == "???")
                {
                    string noms = CountConnectedUsers();
                    Send("Persones conectades: " + noms);
                }
                else if (missatge.StartsWith(PRE_StartGame))
				{
                    // llista de noms per controlar que si tots posen /start el joc començi
                    StartGame();
				}
                else
                {
                    Sockets.Broadcast(_nom + ": " + missatge);
                }
            }

            public override void OnClose()
            {
                // Quan un usuari desconnecta, cal acomiadar-se'n, esborrar-ne el SocketHandler de la Collection i notificar a la resta que marxa
                Send("Adéu " + _nom + "...");
                Sockets.Remove(this);
                Sockets.Broadcast(PRE_UsersOnline + CountConnectedUsers());
                Sockets.Broadcast(_nom + " s'ha desconnectat.");
            }

            private string CountConnectedUsers()
            {
                string nomsTxt = "";
                foreach (SocketHandler persona in Sockets)
                {
                    nomsTxt += persona._nom + ",";
                }
                try
                {
                    return nomsTxt.Substring(0, nomsTxt.Length - 1);
                }
                catch (ArgumentOutOfRangeException e)
                {
                    Console.WriteLine(e.Message);
                    return "";
                }

            }

            private void StartGame()
			{
                // donar dues cartes a cada jugador
                // mostrar una carta perque els jugadors la agafin
                // al cap d'un temps que la tregui i en mostri una altra
			}


            private void sendInitialCards()
            {
                Send("/givecard " + Baralla.ElementAt(random.Next(Baralla.Count)));
                Send("/givecard " + Baralla.ElementAt(random.Next(Baralla.Count)));

            }
        }
    }
}