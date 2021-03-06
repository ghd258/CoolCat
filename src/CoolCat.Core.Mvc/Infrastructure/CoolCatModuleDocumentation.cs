﻿using CoolCat.Core.Attributes;
using CoolCat.Core.Contracts;
using CoolCat.Core.DomainModel;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CoolCat.Core.Mvc.Infrastructure
{
    public class CoolCatModuleDocumentation : IQueryDocumentation
    {
        private Dictionary<string, List<QueryDocumentItem>> _documentations = new Dictionary<string, List<QueryDocumentItem>>();

        public void BuildDocumentation(string moduleName, IDataStoreQuery query)
        {
            if (!_documentations.ContainsKey(moduleName))
            {
                _documentations.Add(moduleName, new List<QueryDocumentItem>());
            }

            _documentations[moduleName].Add(RefactorDocuments(query));

        }

        public Dictionary<string, List<QueryDocumentItem>> GetAllDocuments()
        {
            return _documentations;
        }

        private QueryDocumentItem RefactorDocuments(IDataStoreQuery query)
        {
            var type = query.GetType();

            var responseType = ((ResponseTypeAttribute[])Attribute.GetCustomAttributes(type, typeof(ResponseTypeAttribute))).FirstOrDefault();

            var requestType = ((RequestParameterTypeAttribute[])Attribute.GetCustomAttributes(type, typeof(RequestParameterTypeAttribute))).FirstOrDefault();

            var item = new QueryDocumentItem();
            item.QueryName = query.QueryName;

            if (requestType is NoneRequestParameterAttribute)
            {
                item.RequestSample = "No Request Parameter.";
            }
            else
            {
                item.RequestSample = BuildSampleFromType(requestType.RequestType);
            }

            item.ResponseSample = BuildSampleFromType(responseType.ResponseType);

            return item;
        }

        private string BuildSampleFromType(Type t)
        {
            Object obj = null;

            if (t.IsGenericType)
            {
                var innerType = t.GetGenericArguments()[0];

                var listType = typeof(List<>).MakeGenericType(innerType);
                var list = Activator.CreateInstance(listType);

                var addMethod = listType.GetMethod("Add");
                addMethod.Invoke(list, new object[] { innerType.Assembly.CreateInstance(innerType.FullName) });
            }
            else
            {
                obj = t.Assembly.CreateInstance(t.FullName);
            }

            var sample = JsonConvert.SerializeObject(obj);

            return sample;
        }
    }
}
