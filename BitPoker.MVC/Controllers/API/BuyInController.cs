﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using System.Runtime.Caching;

namespace BitPoker.MVC.Controllers
{
    public class BuyInController : BaseController
    {
        private readonly BitPoker.Repository.IHandRepository repo;
        private readonly BitPoker.Repository.ITableRepository tableRepo;

        public BuyInController()
        {
            this.tableRepo = new Repository.InMemoryTableRepo();
        }

        [HttpPost]
        public BitPoker.Models.Messages.BuyInResponseMessage Post(BitPoker.Models.Messages.BuyInRequestMessage buyInRequest)
        {
            if (!base.Verify(buyInRequest.BitcoinAddress, buyInRequest.ToString(), buyInRequest.Signature))
            {
                throw new Exceptions.SignatureNotValidException();
            }

            BitPoker.Models.Messages.BuyInResponseMessage response = new BitPoker.Models.Messages.BuyInResponseMessage()
            {
                TimeStamp = DateTime.UtcNow
            };

            var table = tableRepo.Find(buyInRequest.TableId);

            if (table != null)
            {
                //Is seat empty?

                table.Players[buyInRequest.Seat] = new BitPoker.Models.TexasHoldemPlayer()
                {
                    BitcoinAddress = buyInRequest.BitcoinAddress,
                    Stack = buyInRequest.Amount,
                    Position = buyInRequest.Seat, //Assume no hand played for this mock
                    IsBigBlind = false,
                    IsDealer = true,
                    IsSmallBlind = false,
                    IsTurnToAct = false,
                };

                tableRepo.Update(table);

                Boolean createHand = false;
                if (createHand)
                {
                    //			
                    const String alice_wif = "93Loqe8T3Qn3fCc87AiJHYHJfFFMLy6YuMpXzffyFsiodmAMCZS";
                    NBitcoin.BitcoinSecret alice_secret = new NBitcoin.BitcoinSecret(alice_wif, NBitcoin.Network.TestNet);
                    NBitcoin.BitcoinAddress alice_address = alice_secret.GetAddress();

                    //response.Table = table;


                    //Create players
                    BitPoker.Models.PlayerInfo[] players = new BitPoker.Models.PlayerInfo[2];
                    players[0] = new BitPoker.Models.PlayerInfo() { BitcoinAddress = alice_address.ToString(), UserAgent = "Bitpoker 0.1", IPAddress = "https://bitpoker.azurewebsites.net/api" };
                    players[1] = new BitPoker.Models.PlayerInfo() { BitcoinAddress = buyInRequest.BitcoinAddress };

                    //Alice in seat 0, you in the sb
                    //response.Players[0] = new BitPoker.Models.PlayerInfo() { BitcoinAddress = alice_address.ToString(), UserAgent = "Bitpoker 0.1", IPAddress = "https://bitpoker.azurewebsites.net/api" };
                    //response.Players[1] = new BitPoker.Models.PlayerInfo() { BitcoinAddress = buyInRequest.BitcoinAddress };


                    //Alice pub key
                    const String alice_pub_key = "041FA97EFD760F26E93E91E29FDDF3DDDDD3F543841CF9435BDC156FB73854F4BF22557798BA535A3EE89A62238C5AFC7F8BF1FA0985DC4E1A06C25209BAB78BD1";
                    Byte[] alicePubKeyAsBytes = NBitcoin.DataEncoders.Encoders.Hex.DecodeData(alice_pub_key);

                    NBitcoin.PubKey alicePubKey = new NBitcoin.PubKey(alicePubKeyAsBytes);
                    NBitcoin.PubKey userKey = new NBitcoin.PubKey(buyInRequest.PubKey);

                    var scriptPubKey = NBitcoin.PayToMultiSigTemplate.Instance.GenerateScriptPubKey(2, new[] { alicePubKey, userKey });

                    //As its heads up, create the first hand and deck
                    BitPoker.Models.Hand hand = new BitPoker.Models.Hand(players);
                }

                return response;
            }
            else
            {
                throw new Exceptions.TableNotFoundException();
            }
        }
    }
}