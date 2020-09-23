using System;
using System.Collections.Generic;
using System.IO;
using RES.ResMap.CppGenerator.Data;

namespace RES.ResMap.Mapper
{
    public enum MappingType
    {
        None,
        Texture,
        Sound,
        Music,
        Font,
        Shader
    }

    public class DirectoryMapping
    {
        public DirectoryMapping(MappingType mapping, params (string Key, string Path)[] maps)
        {
            Mapping = mapping;
            FileMapping = maps;
        }

        public MappingType Mapping { get; }
        public (string Key, string Path)[] FileMapping { get; }
    }

    public class OperationResult<T>
    {
        public OperationResult(T data, bool success, string errorMessage = null)
        {
            Data = data;
            Success = success;
            if (!success)
                ErrorMessage = errorMessage;
        }

        public T Data { get; }
        public bool Success { get; }
        public string ErrorMessage { get; }
    }

    public class SfmlMapper
    {
        private const string TexturePath = "texture";
        private const string SoundPath = "sound";
        private const string FontPath = "font";
        private const string MusicPath = "music";
        private const string ShaderPath = "shader";
        private readonly CppStringGenerator _cppStringGenerator = new CppStringGenerator();
        private readonly object _outputLock = new object();

        public event Action<string> OutputReceived;

        private void SendOutput(string output)
        {
            lock (_outputLock)
            {
                OutputReceived?.Invoke(output);
            }
        }

        public void MapDirectory(string path)
        {
            var result = OpenDirectory(path);
            if (!result.Success) SendOutput("Error: " + result.ErrorMessage);
            var directories = result.Data;

            foreach (var directory in directories)
            {
                var directoryInfo = new DirectoryInfo(directory);
                MappingType mappingType;
                var directoryName = directoryInfo.Name.ToLower();

                if (directoryName.StartsWith(TexturePath))
                    mappingType = MappingType.Texture;
                else if (directoryName.StartsWith(FontPath))
                    mappingType = MappingType.Font;
                else if (directoryName.StartsWith( SoundPath))
                    mappingType = MappingType.Sound;
                else if (directoryName.StartsWith(MusicPath))
                    mappingType = MappingType.Music;
                else if (directoryName.StartsWith(ShaderPath))
                    mappingType = MappingType.Shader;
                else
                    mappingType = MappingType.None;

                if (mappingType != MappingType.None)
                {
                    var files = directoryInfo.GetFiles();
                    var mapInfo = new List<DirectoryMapping>();
                    foreach (var fileInfo in files)
                    {
                        var key = Path.GetFileNameWithoutExtension(fileInfo.Name).ToUpper();
                        var filePath = $"./content/{directoryInfo.Name}/{fileInfo.Name}";
                        SendOutput($"Mapping: {key} to {filePath}");
                        mapInfo.Add(new DirectoryMapping(mappingType, (key, filePath)));
                    }

                }
            }
        }

        private OperationResult<IEnumerable<string>> OpenDirectory(string path)
        {
            try
            {
                var directories = Directory.GetDirectories(path);
                SendOutput($"Found Directories: {string.Join('\n', directories)}");
                return new OperationResult<IEnumerable<string>>(directories, true);
            }
            catch (Exception e)
            {
                return new OperationResult<IEnumerable<string>>(null, false, e.ToString());
            }
        }
    }
}