using System;
using System.IO;
using Spring.Util;

namespace Spring.TestSupport
{
    /// <summary>
    /// Test environment implementation.
    /// </summary>
    /// <author>Erich Eichinger</author>
    internal class VirtualEnvironmentMock : IVirtualEnvironment, IDisposable
    {
        private readonly IVirtualEnvironment _prevEnvironment;

        private readonly string _currentVirtualFilePath;
        private readonly string _pathInfo;
        private string _currentExecutionFilePath;
        private readonly string _applicationVirtualPath;

        public VirtualEnvironmentMock(string currentVirtualFilePath, string pathInfo, string applicationVirtualPath, bool autoInitialize)
        {
            _currentVirtualFilePath = currentVirtualFilePath;
            _pathInfo = (pathInfo == null || pathInfo.Length == 0) ? "" : "/" + pathInfo.TrimStart('/'); // prevent null string and ensure '/' prefixed
            _applicationVirtualPath = "/" + ("" + applicationVirtualPath).Trim('/');
            if (!_applicationVirtualPath.EndsWith("/")) _applicationVirtualPath = _applicationVirtualPath + "/";

            _prevEnvironment = VirtualEnvironment.SetInstance(this);
            if (autoInitialize)
            {
                VirtualEnvironment.SetInitialized();
            }
        }

        public string ApplicationVirtualPath
        {
            get { return _applicationVirtualPath; }
        }

        public string CurrentVirtualPath
        {
            get
            {
                return _currentVirtualFilePath + _pathInfo;
            }
        }

        public string CurrentVirtualFilePath
        {
            get { return _currentVirtualFilePath; }
        }

        public string CurrentExecutionFilePath
        {
            get { return this._currentExecutionFilePath; }
            set { this._currentExecutionFilePath = value; }
        }

        public string MapPath(string virtualPath)
        {
            string basePath = Path.GetDirectoryName(new Uri(GetType().Assembly.CodeBase).LocalPath);
            string resultPath = WebUtils.CreateAbsolutePath(this.ApplicationVirtualPath, virtualPath);
            resultPath = basePath.TrimEnd('\\') + "\\" + resultPath.Replace('/', '\\').TrimStart('\\');
            return resultPath;
        }

        public void Dispose()
        {
            VirtualEnvironment.SetInstance(_prevEnvironment);
        }
    }
}