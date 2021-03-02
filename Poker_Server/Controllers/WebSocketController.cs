using Microsoft.Web.WebSockets;
using Newtonsoft.Json;
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

        #region Command prefixes
        private static readonly string PRE_UsersOnline = "/online";
        private static readonly string PRE_ShowCard = "/showcard";
        private static readonly string PRE_StartGame = "/start";
        private static readonly string PRE_SendCard = "/sendcard";
        private static readonly string PRE_DemandCard = "/card";
        private static readonly string PRE_ResetGame = "/reset";
        private static readonly string PRE_CloseConnection = "/youshallnotpass";
        #endregion

        private static List<string> wantStart = new List<string>();
        private static List<Card>[] playerHands = new List<Card>[6];
        private static List<int> playerIds = new List<int> { 0, 1, 2, 3, 4, 5 };
        private static int idxCarta = 0;
        private static bool isPlaying = false;

        private Random random = new Random();
        private static List<Card> Baralla = Cards.GenerarBaralla();

        private static int countdownSecs = 3;

        public static Thread thread;
        public static Thread tShowCards;

        private class SocketHandler : WebSocketHandler
        {
			private static readonly WebSocketCollection Sockets = new WebSocketCollection();

            private readonly string _nom;
            private readonly int _id;

            public SocketHandler(string nom)
            {
                _nom = nom;
                _id = playerIds[0];
                playerIds.Remove(_id);
            }

            public override void OnOpen()
            {
				if (Sockets.Count >= 6)
				{
                    Send("Server is full. Try again later...");
                    Send(PRE_CloseConnection);
				}
                else if (isPlaying)
				{
                    Send("A match is being played right now. Try again later...");
                    Send(PRE_CloseConnection);
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
                    Send("Connected! (" + _id + ")");
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
                else if (missatge.StartsWith(PRE_StartGame) && !isPlaying)
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
                else if (missatge.StartsWith(PRE_DemandCard))
				{
					if (tShowCards.IsAlive)
					{
                        tShowCards.Abort();
                        idxCarta--;
                        SendCard(this);
                        bool isFinished = true;
						for (int i = 0; i < Sockets.Count; i++)
						{
							if (playerHands[i].Count < 5)
							{
                                isFinished = false;
							}
						}
						if (isFinished)
						{
                            Sockets.Broadcast("Tothom esta ple...");
                            // tots tenen 5 cartes, a contar :)
                            GetResult();
						}
                        else
						{
                            tShowCards = new Thread(new ThreadStart(ShowCards));
                            tShowCards.Start();
                        }
					}
				}
                else
                {
                    Sockets.Broadcast(_nom + ": " + missatge);
                }
            }

            public override void OnClose()
            {
				if (Sockets.Contains(this)) // Si el que es desconnecta realment s'ha connectat
				{
                    Sockets.Remove(this);
                    Sockets.Broadcast(PRE_UsersOnline + CountConnectedUsers());
                    Sockets.Broadcast(_nom + " has disconnected."); // !!! quan està ple el que s'intenta connectar pero no hi ha lloc, si es desconnecta surt aixo igualment !!!
                    playerIds.Add(_id);
                    playerIds.Sort();
                    if (thread != null && thread.IsAlive)
                    {
                        thread.Abort();
                        Sockets.Broadcast("Countdown cancelled...");
                        wantStart.Remove(_nom);
                    }
                    if (tShowCards != null && tShowCards.IsAlive)
                    {
                        tShowCards.Abort();
                    }
                    if (isPlaying)
                    {
                        isPlaying = false;
                        Sockets.Broadcast(PRE_ResetGame); // Dir als clients que resetegin la interficie
                        idxCarta = 0;
                        Baralla = Cards.GenerarBaralla();
                    }
                }
            }

            private void GetResult()
			{
				for (int i = 0; i < Sockets.Count; i++)
				{
                    Sockets.Broadcast("Tothom ple, a contar! :)");
                    
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
                isPlaying = true;
                Sockets.Broadcast("Game started :)");
                wantStart.Clear();

                
                for (int i = 0; i < playerHands.Length; i++)
				{
                    playerHands[i] = new List<Card>(5);
				}

                SendInitialCards();
                tShowCards = new Thread(new ThreadStart(ShowCards));
                tShowCards.Start();
                
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
                    //Envia les dues cartes inicials al player
                    SendCard((SocketHandler)player);
                    SendCard((SocketHandler)player);
                }
            }

            private void ShowCards()
			{
                while (true)
				{
                    ShowCard();
                    Thread.Sleep(3495);
                }
			}
            
            private void SendCard(SocketHandler player)
			{
                //Comprova que no es surti de la baralla
                if (idxCarta < Baralla.Count())
                {
                    //Envia la carta al player
                    playerHands[player._id].Add(Baralla[idxCarta]);
                    string json = JsonConvert.SerializeObject(Baralla[idxCarta]);
                    player.Send(PRE_SendCard + json);
                    Console.WriteLine(Baralla.ElementAt(idxCarta));
                    //Elimina aquesta carta de la baralla
                    Baralla.RemoveAt(idxCarta);

                } else
				{
                    idxCarta = 0;
				}
			}

            private void ShowCard()
			{
				if (idxCarta < Baralla.Count())
				{
                    string json = JsonConvert.SerializeObject(Baralla[idxCarta++]);
                    Sockets.Broadcast(PRE_ShowCard + json);
                }
                else
				{
                    idxCarta = 0;
				}
			}
        }
    }
}