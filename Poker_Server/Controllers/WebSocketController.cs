using Microsoft.Web.WebSockets;
using Poker_Server.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
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

            
            HttpContext.Current.AcceptWebSocketRequest(new SocketHandler(nom));
            return new HttpResponseMessage(HttpStatusCode.SwitchingProtocols);


        }
        private static List<string> wantStart = new List<string>();
        private static int idxCarta = 0;
        private class SocketHandler : WebSocketHandler
        {

            
			private static readonly WebSocketCollection Sockets = new WebSocketCollection();

            private readonly string _nom;

            private Random random = new Random();
            private static List<string> Baralla = Cards.GenerarBaralla();
            
            private int countdownSecs = 5;

            Thread thread;
            Thread tShowCards; // thread per anar donant cartes

            public SocketHandler(string nom)
            {
                _nom = nom;
            }

            public override void OnOpen()
            {
				if (Sockets.Count >= 6)
				{
                    Send("Server is full. Try again later...");
                    
				}
                else
				{
                    if (thread != null && thread.IsAlive)
                    {
                        thread.Abort();
                        Sockets.Broadcast("Countdown cancelled...");
                    }
                    Sockets.Broadcast(_nom + " has connected.");
                    Sockets.Add(this);
                    Send("Connected!");
                    Sockets.Broadcast(PRE_UsersOnline + CountConnectedUsers());
                    //Sockets.Broadcast("/showcard " + Baralla.ElementAt(random.Next(Baralla.Count)));
                    if (Sockets.Count == 2)
                    {
                        Sockets.Broadcast("You can start the game typing /start in the chat...");
                    }
                    else if (Sockets.Count > 2)
					{
                        Send("You can start the game typing /start in the chat...");
					}
                }
            }

            public override void OnMessage(string missatge)
            {
                if (missatge == PRE_UsersOnline)
                {
                    string noms = CountConnectedUsers();
                    Send("Online users: " + noms);
                }
                else if (missatge.StartsWith(PRE_StartGame))
				{
					if (!wantStart.Contains(_nom))
					{
                        wantStart.Add(_nom);
					}
                    else
					{
                        Send("You already voted to start...");
					}
					if (Sockets.Count > 1 && wantStart.Count == Sockets.Count)
					{
                        thread = new Thread(new ThreadStart(Countdown));
                        countdownSecs = 5;
                        thread.Start(); // start the game after 5 secs
                    }
					else {
                        Sockets.Broadcast("Petition to start the game (" + wantStart.Count + "/" + Sockets.Count + ")");
                    }
				}
                else
                {
                    Sockets.Broadcast(_nom + ": " + missatge);
                }
            }

            public override void OnClose()
            {
                Sockets.Remove(this);
                Sockets.Broadcast(PRE_UsersOnline + CountConnectedUsers());
                Sockets.Broadcast(_nom + " has disconnected."); // !!! quan està ple el que s'intenta connectar pero no hi ha lloc, si es desconnecta surt aixo igualment !!!
				if (thread != null && thread.IsAlive)
				{
                    thread.Abort();
                    Sockets.Broadcast("Countdown cancelled...");
                    wantStart.Remove(_nom);
				}
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
                Sockets.Broadcast("Game started :)");
                wantStart.Clear();

                SendInitialCards();
                ShowCard();
			}

            private void Countdown()
			{
				for (int i = countdownSecs; i > 0; i--)
				{
                    Sockets.Broadcast("Starting the game in " + i);
                    Thread.Sleep(1000);
				}
                StartGame();
			}


            private void SendInitialCards()
            {
				foreach (var player in Sockets)
				{
                    player.Send(PRE_SendCard + Baralla.ElementAt(idxCarta++));
                    player.Send(PRE_SendCard + Baralla.ElementAt(idxCarta++));
                    //SendCard();
                    //SendCard();
				}
            }

            private void SendCard()
			{
                Send(PRE_SendCard + Baralla.ElementAt(idxCarta++)); // !!!!!!!! CONTROLAR QUE NO ARRIBI AL FINAL DE LA BARALLA
			}

            private void ShowCard()
			{
                Sockets.Broadcast(PRE_ShowCard + Baralla.ElementAt(idxCarta++));// !!!!! same que send card
			}
        }
    }
}