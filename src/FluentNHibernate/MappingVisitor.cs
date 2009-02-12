﻿using System;
using System.Xml;
using NHibernate.Cfg;

namespace FluentNHibernate
{
    public class MappingVisitor : IMappingVisitor
    {
        private readonly Configuration _configuration;
        private readonly Conventions _conventions;
        

        public MappingVisitor(Conventions conventions, Configuration configuration)
        {
            _conventions = conventions;
            _configuration = configuration;
        }

		public MappingVisitor() : this(new Conventions(), new Configuration())
		{
		}

        #region IMappingVisitor Members

        public Conventions Conventions
        {
            get { return _conventions; }
        }

        public Type CurrentType { get; set;}

        public virtual void AddMappingDocument(XmlDocument document, Type type)
        {
            if (_configuration.GetClassMapping(type) == null)
                _configuration.AddDocument(document);
        }

        #endregion
    }
}
