﻿// Copyright (c) .NET Core Community. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using DotNetCore.CAP.Internal;
using DotNetCore.CAP.Messages;
using DotNetCore.CAP.Monitoring;
using DotNetCore.CAP.Persistence;

namespace DotNetCore.CAP.InMemoryStorage
{
    internal class InMemoryMonitoringApi : IMonitoringApi
    {
        public Task<MediumMessage?> GetPublishedMessageAsync(long id)
        {
            return Task.FromResult<MediumMessage?>(InMemoryStorage.PublishedMessages.Values.FirstOrDefault(x => x.DbId == id.ToString(CultureInfo.InvariantCulture)));
        }

        public Task<MediumMessage?> GetReceivedMessageAsync(long id)
        {
            return Task.FromResult<MediumMessage?>(InMemoryStorage.ReceivedMessages.Values.FirstOrDefault(x => x.DbId == id.ToString(CultureInfo.InvariantCulture)));
        }

        public Task<StatisticsDto> GetStatisticsAsync()
        {
            var stats = new StatisticsDto
            {
                PublishedSucceeded = InMemoryStorage.PublishedMessages.Values.Count(x => x.StatusName == StatusName.Succeeded),
                ReceivedSucceeded = InMemoryStorage.ReceivedMessages.Values.Count(x => x.StatusName == StatusName.Succeeded),
                PublishedFailed = InMemoryStorage.PublishedMessages.Values.Count(x => x.StatusName == StatusName.Failed),
                ReceivedFailed = InMemoryStorage.ReceivedMessages.Values.Count(x => x.StatusName == StatusName.Failed),
                PublishedDelayed = InMemoryStorage.PublishedMessages.Values.Count(x => x.StatusName == StatusName.Delayed)
            };
            return Task.FromResult(stats);
        }

        public Task<IDictionary<DateTime, int>> HourlyFailedJobs(MessageType type)
        {
            return GetHourlyTimelineStats(type, nameof(StatusName.Failed));
        }

        public Task<IDictionary<DateTime, int>> HourlySucceededJobs(MessageType type)
        {
            return GetHourlyTimelineStats(type, nameof(StatusName.Succeeded));
        }

        public Task<PagedQueryResult<MessageDto>> GetMessagesAsync(MessageQueryDto queryDto)
        {
            if (queryDto.MessageType == MessageType.Publish)
            {
                var expression = InMemoryStorage.PublishedMessages.Values.Where(x => true);

                if (!string.IsNullOrEmpty(queryDto.StatusName))
                {
                    expression = expression.Where(x => x.StatusName.ToString().Equals(queryDto.StatusName, StringComparison.InvariantCultureIgnoreCase));
                }

                if (!string.IsNullOrEmpty(queryDto.Name))
                {
                    expression = expression.Where(x => x.Name.Equals(queryDto.Name, StringComparison.InvariantCultureIgnoreCase));
                }

                if (!string.IsNullOrEmpty(queryDto.Content))
                {
                    expression = expression.Where(x => x.Content.Contains(queryDto.Content));
                }

                var offset = queryDto.CurrentPage * queryDto.PageSize;
                var size = queryDto.PageSize;

                var allItems = expression.Select(x => new MessageDto()
                {
                    Added = x.Added,
                    Version = "N/A",
                    Content = x.Content,
                    ExpiresAt = x.ExpiresAt,
                    Id = x.DbId,
                    Name = x.Name,
                    Retries = x.Retries,
                    StatusName = x.StatusName.ToString()
                });

                return Task.FromResult( new PagedQueryResult<MessageDto>()
                {
                    Items = allItems.Skip(offset).Take(size).ToList(),
                    PageIndex = queryDto.CurrentPage,
                    PageSize = queryDto.PageSize,
                    Totals = allItems.Count()
                });
            }
            else
            {
                var expression = InMemoryStorage.ReceivedMessages.Values.Where(x => true);

                if (!string.IsNullOrEmpty(queryDto.StatusName))
                {
                    expression = expression.Where(x => x.StatusName.ToString().Equals(queryDto.StatusName, StringComparison.InvariantCultureIgnoreCase));
                }

                if (!string.IsNullOrEmpty(queryDto.Name))
                {
                    expression = expression.Where(x => x.Name.Equals(queryDto.Name, StringComparison.InvariantCultureIgnoreCase));
                }

                if (!string.IsNullOrEmpty(queryDto.Group))
                {
                    expression = expression.Where(x => x.Group.Equals(queryDto.Group, StringComparison.InvariantCultureIgnoreCase));
                }

                if (!string.IsNullOrEmpty(queryDto.Content))
                {
                    expression = expression.Where(x => x.Content.Contains(queryDto.Content));
                }

                var offset = queryDto.CurrentPage * queryDto.PageSize;
                var size = queryDto.PageSize;

                var allItems = expression.Select(x => new MessageDto()
                {
                    Added = x.Added,
                    Group = x.Group,
                    Version = "N/A",
                    Content = x.Content,
                    ExpiresAt = x.ExpiresAt,
                    Id = x.DbId,
                    Name = x.Name,
                    Retries = x.Retries,
                    StatusName = x.StatusName.ToString()
                });

                return Task.FromResult(new PagedQueryResult<MessageDto>()
                {
                    Items = allItems.Skip(offset).Take(size).ToList(),
                    PageIndex = queryDto.CurrentPage,
                    PageSize = queryDto.PageSize,
                    Totals = allItems.Count()
                });
            }
        }

        public ValueTask<int> PublishedFailedCount()
        {
            return new ValueTask<int>(InMemoryStorage.PublishedMessages.Values.Count(x => x.StatusName == StatusName.Failed));
        }

        public ValueTask<int> PublishedSucceededCount()
        {
            return new ValueTask<int>(InMemoryStorage.PublishedMessages.Values.Count(x => x.StatusName == StatusName.Succeeded));
        }

        public ValueTask<int> ReceivedFailedCount()
        {
            return new ValueTask<int>(InMemoryStorage.ReceivedMessages.Values.Count(x => x.StatusName == StatusName.Failed));
        }

        public ValueTask<int> ReceivedSucceededCount()
        {
            return new ValueTask<int>(InMemoryStorage.ReceivedMessages.Values.Count(x => x.StatusName == StatusName.Succeeded));
        }

        private Task<IDictionary<DateTime, int>> GetHourlyTimelineStats(MessageType type, string statusName)
        {
            var endDate = DateTime.Now;
            var dates = new List<DateTime>();
            for (var i = 0; i < 24; i++)
            {
                dates.Add(endDate);
                endDate = endDate.AddHours(-1);
            }

            var keyMaps = dates.ToDictionary(x => x.ToString("yyyy-MM-dd-HH"), x => x);


            Dictionary<string, int> valuesMap;
            if (type == MessageType.Publish)
            {
                valuesMap = InMemoryStorage.PublishedMessages.Values
                    .Where(x => x.StatusName.ToString() == statusName)
                    .GroupBy(x => x.Added.ToString("yyyy-MM-dd-HH"))
                    .ToDictionary(x => x.Key, x => x.Count());
            }
            else
            {
                valuesMap = InMemoryStorage.ReceivedMessages.Values
                    .Where(x => x.StatusName.ToString() == statusName)
                    .GroupBy(x => x.Added.ToString("yyyy-MM-dd-HH"))
                    .ToDictionary(x => x.Key, x => x.Count());
            }

            foreach (var key in keyMaps.Keys)
            {
                if (!valuesMap.ContainsKey(key))
                {
                    valuesMap.Add(key, 0);
                }
            }

            IDictionary<DateTime, int> result = new Dictionary<DateTime, int>();
            for (var i = 0; i < keyMaps.Count; i++)
            {
                var value = valuesMap[keyMaps.ElementAt(i).Key];
                result.Add(keyMaps.ElementAt(i).Value, value);
            }
            return Task.FromResult(result);
        }
    }
}