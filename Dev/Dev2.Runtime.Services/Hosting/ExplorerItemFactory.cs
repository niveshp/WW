/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2016 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Dev2.Common;
using Dev2.Common.Interfaces.Data;
using Dev2.Common.Interfaces.Explorer;
using Dev2.Common.Interfaces.Runtime;
using Dev2.Common.Interfaces.Wrappers;
using Dev2.Explorer;
using Dev2.Runtime.Interfaces;
using Dev2.Runtime.Security;
using Dev2.Services.Security;

namespace Dev2.Runtime.Hosting
{
    public class ExplorerItemFactory : IExplorerItemFactory
    {
        private readonly IAuthorizationService _authService;
        private static readonly string RootName = Environment.MachineName;
        public ExplorerItemFactory(IResourceCatalog catalogue, IDirectory directory, IAuthorizationService authService)
        {
            _authService = authService;
            Catalogue = catalogue;
            Directory = directory;
        }


        //List<string> IExplorerItemFactory.GetDuplicatedResourcesPaths()
        //{
        //    throw new NotImplementedException();
        //}

        public List<string> GetDuplicatedResourcesPaths()
        {
            var stringBuilder = new StringBuilder();
            var resourceList = Catalogue.GetDuplicateResources();
            if (resourceList == null ||resourceList.Count <= 0)
                return new List<string>();
            foreach (var duplicateResource in resourceList)
            {
                stringBuilder.Append(string.Format(" Resource {0} in path {1} and path {2} are the same",
                    duplicateResource.ResourceName, duplicateResource.FilePath, duplicateResource.FilePath2));
                stringBuilder.AppendLine();
            }
            return new List<string> { stringBuilder.ToString() };
        }

        public IExplorerItem CreateRootExplorerItem(string workSpacePath, Guid workSpaceId)
        {
            var resourceList = Catalogue.GetResourceList(workSpaceId);
            var rootNode = BuildStructureFromFilePathRoot(Directory, workSpacePath, BuildRoot());
            AddChildren(rootNode, resourceList);
            return rootNode;
        }

        public IExplorerItem CreateRootExplorerItem(string type, string workSpacePath, Guid workSpaceId)
        {
            var resourceList = Catalogue.GetResourceList(workSpaceId);
            var rootNode = BuildStructureFromFilePathRoot(Directory, workSpacePath, BuildRoot());
            if (type == "Folder")
            {
                return rootNode;
            }
            AddChildren(rootNode, resourceList, type);
            return rootNode;
        }
        private void AddChildren(IExplorerItem rootNode, IEnumerable<IResource> resourceList, string type)
        {
            // ReSharper disable PossibleMultipleEnumeration

            var children = resourceList.Where(a => GetResourceParent(a.ResourcePath) == rootNode.ResourcePath && a.ResourceType == type.ToString());
            // ReSharper restore PossibleMultipleEnumeration
            foreach (var node in rootNode.Children)
            {
                // ReSharper disable PossibleMultipleEnumeration
                AddChildren(node, resourceList, type);
                // ReSharper restore PossibleMultipleEnumeration
            }
            foreach (var resource in children)
            {
                var childNode = CreateResourceItem(resource);
                rootNode.Children.Add(childNode);
            }
        }

        string GetResourceParent(string resourcePath)
        {
            if (String.IsNullOrEmpty(resourcePath))
            {
                return "";
            }
            return resourcePath.Contains("\\") ? resourcePath.Substring(0, resourcePath.LastIndexOf("\\", StringComparison.Ordinal)) : "";
        }

        private void AddChildren(IExplorerItem rootNode, IEnumerable<IResource> resourceList)
        {
            // ReSharper disable PossibleMultipleEnumeration
            var children = resourceList.Where(a => GetResourceParent(a.ResourcePath).Equals(rootNode.ResourcePath, StringComparison.InvariantCultureIgnoreCase));
            // ReSharper restore PossibleMultipleEnumeration
            foreach (var node in rootNode.Children)
            {
                // ReSharper disable PossibleMultipleEnumeration
                AddChildren(node, resourceList);
                // ReSharper restore PossibleMultipleEnumeration
            }
            foreach (var resource in children)
            {
                if (resource.ResourceType == "ReservedService")
                {
                    continue;
                }
                var childNode = CreateResourceItem(resource);
                rootNode.Children.Add(childNode);
            }
        }

        public ServerExplorerItem CreateResourceItem(IResource resource)
        {
            Guid resourceId = resource.ResourceID;
            var childNode = new ServerExplorerItem(resource.ResourceName, resourceId, resource.ResourceType, null, _authService.GetResourcePermissions(resourceId), resource.ResourcePath, "", "")
            {
                IsReservedService = resource.IsReservedService,
                IsService = resource.IsService,
                IsSource = resource.IsSource,
                IsFolder = resource.IsFolder,
                IsServer = resource.IsServer,
                IsResourceVersion = resource.IsResourceVersion,

            };
            return childNode;
        }


        private IExplorerItem BuildStructureFromFilePathRoot(IDirectory directory, string path, IExplorerItem root)
        {

            root.Children = BuildStructureFromFilePath(directory, path, path);
            return root;
        }


        private IList<IExplorerItem> BuildStructureFromFilePath(IDirectory directory, string path, string rootPath)
        {
            var firstGen =
                directory.GetDirectories(path)
                         .Where(a => !a.EndsWith("VersionControl"));

            IList<IExplorerItem> children = new List<IExplorerItem>();
            foreach (var resource in firstGen)
            {
                var resourcePath = resource.Replace(rootPath, "").Substring(1);

                var node = new ServerExplorerItem(new DirectoryInfo(resource).Name, Guid.NewGuid(), "Folder", null, _authService.GetResourcePermissions(Guid.Empty), resourcePath, "", "")
                {
                    IsFolder = true
                };
                children.Add(node);
                node.Children = BuildStructureFromFilePath(directory, resource, rootPath);
            }
            return children;
        }




        public IExplorerItem BuildRoot()
        {
            ServerExplorerItem serverExplorerItem = new ServerExplorerItem(RootName, Guid.Empty, "Server", new List<IExplorerItem>(), _authService.GetResourcePermissions(Guid.Empty), "", "", "")
            {
                ServerId = HostSecurityProvider.Instance.ServerID,
                WebserverUri = EnvironmentVariables.WebServerUri,
                IsServer = true
            };
            return serverExplorerItem;
        }
        public IResourceCatalog Catalogue { get; private set; }
        public IDirectory Directory { get; private set; }

    }

}