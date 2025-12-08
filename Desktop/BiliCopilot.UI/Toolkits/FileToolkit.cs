// Copyright (c) Bili Copilot. All rights reserved.

using Microsoft.Windows.Storage;
using Richasy.WinUIKernel.Share.Toolkits;

namespace BiliCopilot.UI.Toolkits;

internal sealed class FileToolkit : SharedFileToolkit
{
    private static readonly string path = ApplicationData.GetDefault().LocalFolder.Path;
    private static readonly string pathTag = path + "\\TagFilterList.txt";
    private static readonly string pathUid = path + "\\UIDFilterList.txt";
    private static readonly List<string> _tagFilterList = new();
    private static readonly List<string> _uidFilterList = new();

    public static List<string> TagFilterList
    {
        get => _tagFilterList;
    }
    public static List<string> UidFilterList
    {
        get => _uidFilterList;
    }

    /// <summary>
    /// 本地数据是否存在.
    /// </summary>
    /// <returns>结果.</returns>
    public static bool IsLocalDataExist(string fileName, string folderName = "")
    {
        var folder = ApplicationData.GetDefault().LocalFolder;
        var path = string.IsNullOrEmpty(folderName)
            ? Path.Combine(folder.Path, fileName)
            : Path.Combine(folder.Path, folderName, fileName);
        return File.Exists(path);
    }

    public static void InitTagFilter()
    {
        if (!File.Exists(pathTag))
            File.CreateText(pathTag);
        _tagFilterList.AddRange(File.ReadAllText(pathTag).Split(',').ToList());
        if (string.IsNullOrEmpty(_tagFilterList[_tagFilterList.Count - 1]))
            _tagFilterList.RemoveAt(_tagFilterList.Count - 1);
    }

    public static void InitUidFilter()
    {
        if (!File.Exists(pathUid))
            File.CreateText(pathUid);
        _uidFilterList.AddRange(File.ReadAllText(pathUid).Split(',').ToList());
        if (string.IsNullOrEmpty(_uidFilterList[_uidFilterList.Count - 1]))
            _uidFilterList.RemoveAt(_uidFilterList.Count - 1);
    }

    public static async Task AddTag(string tag)
    {
        if (!_tagFilterList.Contains(tag))
        {
            _tagFilterList.Add(tag);
            await File.AppendAllTextAsync(pathTag, tag + ",");
        }
    }

    public static async Task AddUid(string uid)
    {
        _uidFilterList.Add(uid);
        await File.AppendAllTextAsync(pathUid, uid + ",");
    }

}
