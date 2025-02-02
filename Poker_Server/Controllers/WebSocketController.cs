﻿using Microsoft.Web.WebSockets;
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

        private static List<int> wantStart = new List<int>();
        private static List<Card>[] playerHands = new List<Card>[6];
        private static List<int> playerIds = new List<int> { 0, 1, 2, 3, 4, 5 };
        private static int idxCarta = 0;
        private static bool isPlaying = false;
        private static SocketHandler winnerSocket;

        private static List<Card> Baralla = Cards.GenerarBaralla();

        private static int countdownSecs = 3;

        public static Thread thread;
        public static Thread tShowCards;

        private class SocketHandler : WebSocketHandler
        {
			private static readonly WebSocketCollection Sockets = new WebSocketCollection();

            private readonly string _nom;
            public readonly int _id;

            public SocketHandler(string nom)
            {
                _nom = nom;
                _id = playerIds[0];
                playerIds.Remove(_id);
            }

			#region Overrides
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
                    if (Sockets.Count == 2)
                    {
                        Sockets.Broadcast("You can start the game by clicking the start button...");
                    }
                    else if (Sockets.Count > 2)
					{
                        Send("You can start the game by clicking the start button...");
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
					if (!wantStart.Contains(_id))
					{
                        wantStart.Add(_id);
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
                            // tots tenen 5 cartes, a contar :)
                            GetWinner();
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
                    Sockets.Broadcast(_nom + " has disconnected.");
                    playerIds.Add(_id);
                    playerIds.Sort();
                    if (thread != null && thread.IsAlive)
                    {
                        thread.Abort();
                        Sockets.Broadcast("Countdown cancelled...");
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
                    wantStart.Remove(_id);
                }
            }
			#endregion

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

            private void Countdown()
            {
                for (int i = countdownSecs; i > 0; i--)
                {
                    Sockets.Broadcast("Starting the game in " + i);
                    Thread.Sleep(1000);
                }
                StartGame();
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

                //SendInitialCards();
                tShowCards = new Thread(new ThreadStart(ShowCards));
                tShowCards.Start();
			}

            #region Show cards
            private void ShowCards()
            {
                while (true)
                {
                    ShowCard();
                    Thread.Sleep(3495);
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
            #endregion

            #region Send cards
            private void SendInitialCards()
            {
                foreach (var player in Sockets)
                {
                    //Envia les dues cartes inicials al player
                    SendCard((SocketHandler)player);
                    SendCard((SocketHandler)player);
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

                }
                else
                {
                    idxCarta = 0;
                }
            }
            #endregion

            #region Get winner
            private void GetWinner()
            {
                tShowCards.Abort();
                List<int> jugades = new List<int>();
                List<double> valorsFinals = new List<double>();
                for (int i = 0; i < Sockets.Count; i++)
                {
                    // Ordenar la ma per vegades que apareix (desc) i valor (desc)
                    playerHands[i] = playerHands[i].OrderByDescending(x => x.valor).GroupBy(x => x.valor).OrderByDescending(g => g.Count()).SelectMany(g => g).ToList();
                    var valorsDiff = playerHands[i].GroupBy(x => x.valor).Select(x => x.Count()).OrderByDescending(x => x).ToList();

                    var qtyDif = valorsDiff.Count();
                    var maxValue = valorsDiff[0];

                    bool isCorrelatiu = IsCorrelative(playerHands[i].Select(x => x.valor).ToList());
                    bool isMaxCorrelatiu = isCorrelatiu && playerHands[i].Max(x => x.valor) == 14;
                    int palsDiff = playerHands[i].GroupBy(x => x.pal).Count();

                    int playValue = GetPlayValue(qtyDif, maxValue, isCorrelatiu, isMaxCorrelatiu, palsDiff);
                    jugades.Add(playValue);
                    var ma = playerHands[i].GroupBy(x => x.valor).Select(x => x.Key).ToList();
                    string valorFinalStr = string.Format("{0}{1,2:00}{2,2:00},", playValue, ma[0], ma[1]);
                    ma.RemoveAt(1);
                    ma.RemoveAt(0);
                    for (int c = 0; c < ma.Count; c++)
                    {
                        valorFinalStr += string.Format("{0,2:00}", ma[c]);
                    }
                    valorsFinals.Add(double.Parse(valorFinalStr));
                }
                int idGuanyador = valorsFinals.IndexOf(valorsFinals.Max());
                foreach (SocketHandler socket in Sockets)
                {
                    if (socket._id == idGuanyador)
                    {
                        winnerSocket = socket;
                        break;
                    }
                }

                Sockets.Broadcast("Ha guanyat " + winnerSocket._nom + "!!!!");
                winnerSocket.Send("Felicitats! :D");
            }

            private bool IsCorrelative(List<int> list)
			{
                bool isCorrelative = true;
                for (int n = 0; n < list.Count-1; n++)
				{
					if (list[n] - 1 != list[n+1])
					{
                        return false;
					}
				}
                return isCorrelative;
			}

            private int GetPlayValue(int qtyDif, int maxValue, bool isCorrelatiu, bool isMaxCorrelatiu, int palsDiff)
			{
                if (qtyDif == 2 && maxValue == 4)
                {
                    // poker
                    return 7;
                }
                else if (qtyDif == 2 && maxValue == 3)
                {
                    // full
                    return 6;
                }
                else if (qtyDif == 3 && maxValue == 3)
                {
                    // trio
                    return 3;
                }
                else if (qtyDif == 3 && maxValue == 2)
                {
                    // doble parella
                    return 2;
                }
                else if (qtyDif == 4 && maxValue == 2)
                {
                    // parella
                    return 1;
                }
                else if (qtyDif == 5)
                {
                    if (palsDiff == 1)
                    {
                        if (isMaxCorrelatiu)
                        {
                            // escala reial
                            return 9;
                        }
                        else if (isCorrelatiu)
                        {
                            // escala de color
                            return 8;
                        }
                        else if (!isMaxCorrelatiu && !isCorrelatiu)
                        {
                            // color
                            return 5;
                        }
                    }
                    else if (palsDiff > 1 && isCorrelatiu)
                    {
                        // escala
                        return 4;
                    }
                    else
                    {
                        // res
                        return 0;
                    }

                }
                return -1;
            }
            #endregion
        }

    }
}