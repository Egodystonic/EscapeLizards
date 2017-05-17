// All code copyright (c) 2016 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information
// Created on 01 02 2016 at 17:55 by Ben Bowen

using System;
using System.Collections.Generic;
using Ophidian.Losgap;

namespace Egodystonic.EscapeLizards {
	public enum Star {
		Gold = 3,
		Silver = 2,
		Bronze = 1,
		None = 0
	}

	public enum Medal {
		Gold = 3,
		Silver = 2,
		Bronze = 1,
		None = 0
	}

	public class LevelPassDetails {
		public readonly Star Star;
		public readonly int TimeRemainingMs;
		public readonly Medal Medal;
		public readonly int VultureEggsDestroyed;
		public readonly int VultureEggsRemaining;
		public readonly bool GoldenEggReceived;
		public readonly int NumGoldenEggs;
		public readonly bool NewPersonalBest;
		public readonly int? PreviousPersonalBestMs;
		public readonly int TimeTakenMs;
		public readonly LeaderboardPlayer? GoldFriend;
		public readonly LeaderboardPlayer? SilverFriend;
		public readonly LeaderboardPlayer? BronzeFriend;
		public readonly int? GoldFriendTimeRemainingMs;
		public readonly int? SilverFriendTimeRemainingMs;
		public readonly int? BronzeFriendTimeRemainingMs;
		public readonly int CoinsTakenInGame;
		public readonly int CoinsTakenTotal;
		public readonly int FreshCoinsTakenInGame;
		public readonly int TotalCoinsInLevel;
		public readonly bool WasLastLevelInWorld;
		public readonly LevelLeaderboardDetails PreviousLeaderboardDetails;

		public LevelPassDetails(Star star, int timeRemainingMs, Medal medal, int vultureEggsDestroyed, int vultureEggsRemaining, bool goldenEggReceived, int numGoldenEggs, bool newPersonalBest, int? previousPersonalBestMs, int timeTakenMs, LevelLeaderboardDetails prePassLeaderboardDetails, int coinsTakenInGame, int coinsTakenTotal, int freshCoinsTakenInGame, int totalCoinsInLevel, bool wasLastLevelInWorld) {
			Star = star;
			TimeRemainingMs = timeRemainingMs;
			Medal = medal;
			VultureEggsDestroyed = vultureEggsDestroyed;
			VultureEggsRemaining = vultureEggsRemaining;
			GoldenEggReceived = goldenEggReceived;
			NumGoldenEggs = numGoldenEggs;
			NewPersonalBest = newPersonalBest;
			PreviousPersonalBestMs = previousPersonalBestMs;
			TimeTakenMs = timeTakenMs;
			CoinsTakenInGame = coinsTakenInGame;
			CoinsTakenTotal = coinsTakenTotal;
			FreshCoinsTakenInGame = freshCoinsTakenInGame;
			TotalCoinsInLevel = totalCoinsInLevel;
			WasLastLevelInWorld = wasLastLevelInWorld;
			PreviousLeaderboardDetails = prePassLeaderboardDetails;

			Medal playerPreviousMedal = Medal.None;
			if (prePassLeaderboardDetails.GoldFriend.HasValue && prePassLeaderboardDetails.GoldFriend.Value == LeaderboardManager.LocalPlayerDetails) playerPreviousMedal = Medal.Gold;
			else if (prePassLeaderboardDetails.SilverFriend.HasValue && prePassLeaderboardDetails.SilverFriend.Value == LeaderboardManager.LocalPlayerDetails) playerPreviousMedal = Medal.Silver;
			else if (prePassLeaderboardDetails.BronzeFriend.HasValue && prePassLeaderboardDetails.BronzeFriend.Value == LeaderboardManager.LocalPlayerDetails) playerPreviousMedal = Medal.Bronze;

			switch (medal) {
				case Medal.Gold:
					GoldFriend = LeaderboardManager.LocalPlayerDetails;
					GoldFriendTimeRemainingMs = timeRemainingMs;
					if (LeaderboardManager.LocalPlayerDetails == prePassLeaderboardDetails.GoldFriend
						&& prePassLeaderboardDetails.GoldFriendTimeMs.Value > timeRemainingMs) {
						GoldFriendTimeRemainingMs = prePassLeaderboardDetails.GoldFriendTimeMs.Value;
					}
					if (playerPreviousMedal == Medal.Gold) {
						SilverFriend = prePassLeaderboardDetails.SilverFriend;
						BronzeFriend = prePassLeaderboardDetails.BronzeFriend;
						SilverFriendTimeRemainingMs = prePassLeaderboardDetails.SilverFriendTimeMs;
						BronzeFriendTimeRemainingMs = prePassLeaderboardDetails.BronzeFriendTimeMs;
					}
					else if (playerPreviousMedal == Medal.Silver) {
						SilverFriend = prePassLeaderboardDetails.GoldFriend;
						BronzeFriend = prePassLeaderboardDetails.BronzeFriend;
						SilverFriendTimeRemainingMs = prePassLeaderboardDetails.GoldFriendTimeMs;
						BronzeFriendTimeRemainingMs = prePassLeaderboardDetails.BronzeFriendTimeMs;
					}
					else {
						SilverFriend = prePassLeaderboardDetails.GoldFriend;
						BronzeFriend = prePassLeaderboardDetails.SilverFriend;
						SilverFriendTimeRemainingMs = prePassLeaderboardDetails.GoldFriendTimeMs;
						BronzeFriendTimeRemainingMs = prePassLeaderboardDetails.SilverFriendTimeMs;
					}
					break;
				case Medal.Silver:
					if (playerPreviousMedal == Medal.Gold) {
						GoldFriend = prePassLeaderboardDetails.GoldFriend;
						SilverFriend = prePassLeaderboardDetails.SilverFriend;
						BronzeFriend = prePassLeaderboardDetails.BronzeFriend;
						GoldFriendTimeRemainingMs = prePassLeaderboardDetails.GoldFriendTimeMs;
						SilverFriendTimeRemainingMs = prePassLeaderboardDetails.SilverFriendTimeMs;
						BronzeFriendTimeRemainingMs = prePassLeaderboardDetails.BronzeFriendTimeMs;
					}
					else if (playerPreviousMedal == Medal.Silver) {
						SilverFriend = LeaderboardManager.LocalPlayerDetails;
						SilverFriendTimeRemainingMs = timeRemainingMs;
						if (LeaderboardManager.LocalPlayerDetails == prePassLeaderboardDetails.SilverFriend
							&& prePassLeaderboardDetails.SilverFriendTimeMs.Value > timeRemainingMs) {
								SilverFriendTimeRemainingMs = prePassLeaderboardDetails.SilverFriendTimeMs.Value;
						}
						GoldFriend = prePassLeaderboardDetails.GoldFriend;
						BronzeFriend = prePassLeaderboardDetails.BronzeFriend;
						GoldFriendTimeRemainingMs = prePassLeaderboardDetails.GoldFriendTimeMs;
						BronzeFriendTimeRemainingMs = prePassLeaderboardDetails.BronzeFriendTimeMs;
					}
					else {
						SilverFriend = LeaderboardManager.LocalPlayerDetails;
						SilverFriendTimeRemainingMs = timeRemainingMs;
						GoldFriend = prePassLeaderboardDetails.GoldFriend;
						BronzeFriend = prePassLeaderboardDetails.SilverFriend;
						GoldFriendTimeRemainingMs = prePassLeaderboardDetails.GoldFriendTimeMs;
						BronzeFriendTimeRemainingMs = prePassLeaderboardDetails.SilverFriendTimeMs;
					}
					break;
				case Medal.Bronze:
					if (playerPreviousMedal == Medal.Gold || playerPreviousMedal == Medal.Silver) {
						GoldFriend = prePassLeaderboardDetails.GoldFriend;
						SilverFriend = prePassLeaderboardDetails.SilverFriend;
						BronzeFriend = prePassLeaderboardDetails.BronzeFriend;
						GoldFriendTimeRemainingMs = prePassLeaderboardDetails.GoldFriendTimeMs;
						SilverFriendTimeRemainingMs = prePassLeaderboardDetails.SilverFriendTimeMs;
						BronzeFriendTimeRemainingMs = prePassLeaderboardDetails.BronzeFriendTimeMs;
					}
					else {
						BronzeFriend = LeaderboardManager.LocalPlayerDetails;
						BronzeFriendTimeRemainingMs = timeRemainingMs;
						if (LeaderboardManager.LocalPlayerDetails == prePassLeaderboardDetails.BronzeFriend
							&& prePassLeaderboardDetails.BronzeFriendTimeMs.Value > timeRemainingMs) {
							BronzeFriendTimeRemainingMs = prePassLeaderboardDetails.BronzeFriendTimeMs.Value;
						}
						GoldFriend = prePassLeaderboardDetails.GoldFriend;
						SilverFriend = prePassLeaderboardDetails.SilverFriend;
						GoldFriendTimeRemainingMs = prePassLeaderboardDetails.GoldFriendTimeMs;
						SilverFriendTimeRemainingMs = prePassLeaderboardDetails.SilverFriendTimeMs;
					}
					break;
				default:
					GoldFriend = prePassLeaderboardDetails.GoldFriend;
					SilverFriend = prePassLeaderboardDetails.SilverFriend;
					BronzeFriend = prePassLeaderboardDetails.BronzeFriend;
					GoldFriendTimeRemainingMs = prePassLeaderboardDetails.GoldFriendTimeMs;
					SilverFriendTimeRemainingMs = prePassLeaderboardDetails.SilverFriendTimeMs;
					BronzeFriendTimeRemainingMs = prePassLeaderboardDetails.BronzeFriendTimeMs;
					break;
			}
		}
	}
}