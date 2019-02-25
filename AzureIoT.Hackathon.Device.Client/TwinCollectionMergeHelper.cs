﻿namespace AzureIoT.Hackathon.Device.Client
{
    using Microsoft.Azure.Devices.Shared;
    using Newtonsoft.Json.Linq;
    using System;

    public static class TwinCollectionMergeHelper
    {
        public static TwinCollection Merge(TwinCollection current, TwinCollection patch)
        {
            var currentJObject = JObject.Parse(current.ToJson());
            var patchJObject = JObject.Parse(patch.ToJson());

            currentJObject.Merge(patchJObject, new JsonMergeSettings()
            {
                MergeNullValueHandling = MergeNullValueHandling.Merge,
                PropertyNameComparison = StringComparison.Ordinal
            });

            var sanitized = RemoveEmptyChildren((JToken)currentJObject);
            return sanitized.ToObject<TwinCollection>();
        }

        public static JToken RemoveEmptyChildren(JToken token)
        {
            if (token.Type == JTokenType.Object)
            {
                JObject copy = new JObject();
                foreach (JProperty prop in token.Children<JProperty>())
                {
                    JToken child = prop.Value;
                    if (child.HasValues)
                    {
                        child = RemoveEmptyChildren(child);
                    }
                    if (!IsEmpty(child))
                    {
                        copy.Add(prop.Name, child);
                    }
                }
                return copy;
            }
            else if (token.Type == JTokenType.Array)
            {
                JArray copy = new JArray();
                foreach (JToken item in token.Children())
                {
                    JToken child = item;
                    if (child.HasValues)
                    {
                        child = RemoveEmptyChildren(child);
                    }
                    if (!IsEmpty(child))
                    {
                        copy.Add(child);
                    }
                }
                return copy;
            }
            return token;
        }

        public static bool IsEmpty(JToken token)
        {
            return (token.Type == JTokenType.Null);
        }
    }
}
