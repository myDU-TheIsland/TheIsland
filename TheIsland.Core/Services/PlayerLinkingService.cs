// <copyright file="PlayerLinkingService.cs" company="Paul Layne">
// Copyright (c) Paul Layne. All rights reserved.
// </copyright>

namespace TheIsland.Core.Services
{
    using System;
    using System.Threading.Tasks;
    using TheIsland.Core.Classes;
    using TheIsland.Core.Services.SQL;
    using TheIsland.Core.Services.SQL.Entities;

    public class PlayerLinkingService
    {
        private readonly DualPlayerRepository _playerRepository;
        private readonly LinkTokenRepository _linkTokenRepository;
        private readonly UserMappingRepository _userMappingRepository;
        private readonly IIngameMessaging _ingameMessaging;

        public PlayerLinkingService(IIngameMessaging ingameMessaging, DualPlayerRepository playerRepository, LinkTokenRepository linkTokenRepository, UserMappingRepository userMappingRepository)
        {
            this._playerRepository = playerRepository;
            this._linkTokenRepository = linkTokenRepository;
            this._userMappingRepository = userMappingRepository;
            this._ingameMessaging = ingameMessaging;
        }

        public async Task<bool> SendToken(double playerId, double discordId)
        {
            // check for existing token
            LinkToken? token = await this._linkTokenRepository.FindByPlayerId(playerId).ConfigureAwait(false);

            if (token == null)
            {
                // no token found generate new one
                token = new LinkToken { discord_id = discordId, player_id = playerId };
                token.token = Guid.NewGuid().ToString().Substring(0, 8);

                await this._linkTokenRepository.AddAsync(token).ConfigureAwait(false);
            }

            // verify player is online
            var player = await this._playerRepository.GetAsync(playerId).ConfigureAwait(false);

            if (player.connected)
            {
                //send message
                await this._ingameMessaging.SendMessage(Convert.ToUInt64(token.player_id), @$"Your Token is: {token.token}").ConfigureAwait(false);
            }

            return player.connected;
        }

        public async Task<bool> VerifyToken(string token)
        {
            // find the token
            var result = await this._linkTokenRepository.FindByToken(token).ConfigureAwait(false);

            if (result == null)
            {
                return false;
            }

            var newEntry = new UserMapping { discord_id = result.discord_id, dual_id = result.player_id };

            await this._userMappingRepository.AddAsync(newEntry).ConfigureAwait(false);
            await this._linkTokenRepository.RemoveAsync(result.id).ConfigureAwait(false);

            return true;
        }

        public Task<DualPlayer?> FindPlayer(string playerName)
        {
            return this._playerRepository.FindByDisplayName(playerName);
        }

        public async Task<bool> HasPlayerMapping(double discordId)
        {
            var result = await this._userMappingRepository.FindByDiscordId(discordId).ConfigureAwait(false);

            if (result != null)
            {
                var linkResult = await this._linkTokenRepository.FindByPlayerId(result.dual_id).ConfigureAwait(false);

                if (linkResult != null)
                {
                    // clean up result.
                    await this._linkTokenRepository.RemoveAsync(linkResult.id).ConfigureAwait(false);
                }
            }

            return result != null;
        }

        public Task<UserMapping?> GetPlayerMapping(double discordId)
        {
            return this._userMappingRepository.FindByDiscordId(discordId);
        }
    }
}
