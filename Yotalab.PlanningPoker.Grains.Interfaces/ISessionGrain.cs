﻿using System;
using System.Collections.Immutable;
using System.Threading.Tasks;
using Orleans;
using Yotalab.PlanningPoker.Grains.Interfaces.Models;

namespace Yotalab.PlanningPoker.Grains.Interfaces
{
  /// <summary>
  /// Грейн сессии планирования.
  /// </summary>
  public interface ISessionGrain : IGrainWithGuidKey
  {
    Task CreateAsync(string name, IParticipantGrain moderator);

    /// <summary>
    /// Получить статус сессии.
    /// </summary>
    /// <returns>Задача на получение статуса сессии.</returns>
    Task<SessionInfo> StatusAsync();

    /// <summary>
    /// Перевести сессию в начальное состояние.
    /// </summary>
    /// <returns>Задача на перевод сессии в начальное состояние.</returns>
    Task ResetAsync(Guid initiatorId);

    /// <summary>
    /// Начать сессию планирования.
    /// </summary>
    /// <returns>Задача на старт сессии.</returns>
    Task StartAsync(Guid initiatorId);

    /// <summary>
    /// Остановить сессию планирования.
    /// </summary>
    /// <returns>Задача на остановку планирования.</returns>
    Task StopAsync(Guid initiatorId);

    /// <summary>
    /// Завершить сессию планирования.
    /// </summary>
    /// <returns>Задача на завершение сессии.</returns>
    Task FinishAsync(Guid initiatorId);

    /// <summary>
    /// Получить голоса участников.
    /// </summary>
    /// <returns>Список голосов участников.</returns>
    Task<ImmutableArray<ParticipantVote>> ParticipantVotes();

    /// <summary>
    /// Принять голос участника.
    /// </summary>
    /// <param name="participantId">Участник сессии.</param>
    /// <param name="vote">Голос.</param>
    /// <returns>Задача на прием голоса.</returns>
    Task AcceptVote(Guid participantId, Vote vote);

    /// <summary>
    /// Войти в сессию пользователем.
    /// </summary>
    /// <param name="participantInfo">Информация об участнике.</param>
    /// <returns>Задача на вход в сессию.</returns>
    Task Enter(Guid participantId);
  }
}
