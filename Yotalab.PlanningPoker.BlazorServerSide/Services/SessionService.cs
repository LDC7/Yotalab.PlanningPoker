﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Orleans;
using Orleans.Streams;
using Yotalab.PlanningPoker.BlazorServerSide.Services.DTO;
using Yotalab.PlanningPoker.Grains.Interfaces;
using Yotalab.PlanningPoker.Grains.Interfaces.Models;

namespace Yotalab.PlanningPoker.BlazorServerSide.Services
{
  public class SessionService
  {
    private readonly ILogger<SessionService> logger;
    private readonly IClusterClient client;

    public SessionService(ILogger<SessionService> logger, IClusterClient client)
    {
      this.logger = logger;
      this.client = client;
    }

    public async Task<SessionInfo> CreateAsync(string name, Guid participantId)
    {
      var sessionId = Guid.NewGuid();
      var sessionGrain = this.client.GetGrain<ISessionGrain>(sessionId);
      var participantGrain = this.client.GetGrain<IParticipantGrain>(participantId);

      await sessionGrain.CreateAsync(name, participantGrain);
      await participantGrain.Join(sessionId);

      return await sessionGrain.StatusAsync();
    }

    public Task<SessionInfo> GetAsync(Guid sessionId)
    {
      var sessionGrain = this.client.GetGrain<ISessionGrain>(sessionId);
      return sessionGrain.StatusAsync();
    }

    public async Task<IEnumerable<SessionInfo>> ListAsync(Guid participantId)
    {
      var participantGrain = this.client.GetGrain<IParticipantGrain>(participantId);
      var sessionsGrainList = await participantGrain.Sessions();
      return await Task.WhenAll(sessionsGrainList.Select(sessionGrain => sessionGrain.StatusAsync()));
    }

    public async Task<IReadOnlyCollection<ParticipantInfoDTO>> ListParticipants(Guid sessionId)
    {
      var sessionGrain = this.client.GetGrain<ISessionGrain>(sessionId);
      var participantVotes = await sessionGrain.ParticipantVotes();
      var participantInfos = await Task.WhenAll(participantVotes
        .Select(p => this.client.GetGrain<IParticipantGrain>(p.ParticipantId).GetAsync()));

      var result = new List<ParticipantInfoDTO>();
      foreach (var participant in participantVotes)
      {
        var info = participantInfos.SingleOrDefault(p => p.Id == participant.ParticipantId);
        if (info != null)
          result.Add(new ParticipantInfoDTO()
          {
            Id = info.Id,
            Name = info.Name,
            Vote = participant.Vote,
            AvatarUrl = info.AvatarUrl
          });
      }

      return result;
    }

    internal Task StopAsync(Guid sessionId, Guid participantId)
    {
      var sessionGrain = this.client.GetGrain<ISessionGrain>(sessionId);
      return sessionGrain.StopAsync(participantId);
    }

    internal Task FinishAsync(Guid sessionId, Guid participantId)
    {
      var sessionGrain = this.client.GetGrain<ISessionGrain>(sessionId);
      return sessionGrain.FinishAsync(participantId);
    }

    internal Task StartAsync(Guid sessionId, Guid participantId)
    {
      var sessionGrain = this.client.GetGrain<ISessionGrain>(sessionId);
      return sessionGrain.StartAsync(participantId);
    }

    internal Task ResetAsync(Guid sessionId, Guid participantId)
    {
      var sessionGrain = this.client.GetGrain<ISessionGrain>(sessionId);
      return sessionGrain.ResetAsync(participantId);
    }

    public async Task<bool> ParticipantJoined(Guid sessionId, Guid participantId)
    {
      var participantGrain = this.client.GetGrain<IParticipantGrain>(participantId);
      var sessionsGrainList = await participantGrain.Sessions();
      return sessionsGrainList.Any(s => s.GetPrimaryKey() == sessionId);
    }

    public async Task<IReadOnlyCollection<ParticipantVote>> Votes(Guid sessionId)
    {
      var sessionGrain = this.client.GetGrain<ISessionGrain>(sessionId);
      return await sessionGrain.ParticipantVotes();
    }

    public Task<StreamSubscriptionHandle<T>> SubscribeAsync<T>(Guid sessionId, Func<T, Task> action) =>
        this.client.GetStreamProvider("SMS")
            .GetStream<T>(sessionId, typeof(T).FullName)
            .SubscribeAsync(new NotificationsObserver<T>(logger, action));
  }
}
