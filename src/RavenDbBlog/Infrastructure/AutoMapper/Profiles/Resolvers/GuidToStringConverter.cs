﻿using System;
using AutoMapper;

namespace RavenDbBlog.Infrastructure.AutoMapper.Profiles.Resolvers
{
    public class GuidToStringConverter : TypeConverter<Guid, string>
    {
        protected override string ConvertCore(Guid source)
        {
            return source.ToString("N");
        }
    }
}