// Copyright (c) Bili Copilot. All rights reserved.

using BiliCopilot.UI.Models.Constants;
using BiliCopilot.UI.ViewModels.Items;
using Microsoft.Extensions.Logging;
using Richasy.BiliKernel.Models;
using Richasy.BiliKernel.Models.Media;

namespace BiliCopilot.UI.ViewModels.View;

/// <summary>
/// 流行视频页面视图模型.
/// </summary>
public sealed partial class PopularPageViewModel
{
    private async Task LoadRecommendVideosAsync()
    {
        try
        {
            var (videos, nextOffset) = await _service.GetRecommendVideoListAsync(_recommendOffset);
            _recommendOffset = nextOffset;
            TryAddVideos(videos, VideoCardStyle.Recommend);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "尝试加载推荐视频时出错.");
        }
    }

    private async Task LoadHotVideosAsync()
    {
        try
        {
            var (videos, nextOffset) = await _service.GetHotVideoListAsync(_hotOffset);
            _hotOffset = nextOffset;
            TryAddVideos(videos, VideoCardStyle.Hot);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "尝试加载热门视频时出错.");
        }
    }

    private async Task LoadTotalRankVideosAsync()
    {
        try
        {
            var videos = await _service.GetGlobalRankingListAsync();
            TryAddVideos(videos, VideoCardStyle.Rank);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "尝试加载全站排行榜视频时出错.");
        }
    }

    private async Task LoadPartitionRankVideosAsync(Partition partition)
    {
        try
        {
            var videos = await _service.GetPartitionRankingListAsync(partition);
            TryAddVideos(videos, VideoCardStyle.Rank);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"尝试加载 {partition.Name} 分区排行榜视频时出错.");
        }
    }

    private async Task LoadPartitionsAsync()
    {
        IsPartitionLoading = true;

        try
        {
            var partitions = await _service.GetVideoPartitionsAsync();
            if (partitions != null)
            {
                // 去除资讯分区，因为资讯分区没有排行榜.
                foreach (var item in partitions.Where(p => p.Id != "202"))
                {
                    Sections.Add(new PopularRankPartitionViewModel(item));
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "尝试加载视频分区列表时出错.");
        }

        IsPartitionLoading = false;
    }

    private void TryAddVideos(IReadOnlyList<VideoInformation> videos, VideoCardStyle style)
    {
        if (videos is not null)
        {
            foreach (var item in videos)
            {
                if (item.Duration <= 30
                    || _tagFilterList.Count > 0 && TagFilter(item)
                    || _uidFilterList.Count > 0 && _uidFilterList.Contains(item.Publisher.User.Id))
                    continue;
                Videos.Add(new VideoItemViewModel(item, style, removeAction: RemoveVideo));
            }
        }
    }

    private bool TagFilter(VideoInformation item)
    {
        if (item.ExtensionData.TryGetValue("TagName", out object tagName) && _tagFilterList.Contains(tagName.ToString()))
            return true;
        var title = item.ExtensionData.TryGetValue("Title", out object value) ? value.ToString() : "";
        foreach (string tag in _tagFilterList)
        {
            if (_tagFilterList.Contains(tagName) || title.Contains(tag))
                return true;
        }
        return false;
    }

    private void InitFilterList()
    {
        GetTagFilterList();
        GetUidFilterList();
    }

    private void GetTagFilterList()
    {
        if (_tagFilterList.Count > 0)
            return;
        var path = Microsoft.Windows.Storage.ApplicationData.GetDefault().LocalFolder.Path + "\\TagFilterList.txt";
        if (!File.Exists(path))
            File.CreateText(path);
        try
        {
            _tagFilterList.AddRange(File.ReadAllText(path).Split(',').ToList());
            if (string.IsNullOrEmpty(_tagFilterList[_tagFilterList.Count - 1]))
                _tagFilterList.RemoveAt(_tagFilterList.Count - 1);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "尝试读取标签过滤列表时出错.");
        }
    }

    private void GetUidFilterList()
    {
        if (_uidFilterList.Count > 0)
            return;
        var path = Microsoft.Windows.Storage.ApplicationData.GetDefault().LocalFolder.Path + "\\UIDFilterList.txt";
        if (!File.Exists(path))
            File.CreateText(path);
        try
        {
            _uidFilterList.AddRange(File.ReadAllText(path).Split(',').ToList());
            if (string.IsNullOrEmpty(_uidFilterList[_uidFilterList.Count - 1]))
                _uidFilterList.RemoveAt(_uidFilterList.Count - 1);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "尝试读取标签过滤列表时出错.");
        }
    }

    private void RemoveVideo(VideoItemViewModel item)
    {
        Videos.Remove(item);
    }
}
