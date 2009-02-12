﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Xml;
using FluentNHibernate.Mapping;
using NHibernate.Cfg;

namespace FluentNHibernate
{
    public class PersistenceModel
    {
        protected List<IMapping> _mappings = new List<IMapping>();
        private Conventions _conventions = new Conventions();
        private bool _configured = false;

        public PersistenceModel()
        {

        }

        public void ForEach<T>(Action<T> action) where T : class
        {
            foreach (var mapping in _mappings)
            {
                T t = mapping as T;
                if (t != null) action(t);
            }
        }

        protected void addTypeConvention(ITypeConvention convention)
        {
            _conventions.AddTypeConvention(convention);
        }

        protected void addMappingsFromThisAssembly()
        {
            Assembly assembly = findTheCallingAssembly();
            addMappingsFromAssembly(assembly);
        }

        public void addMappingsFromAssembly(Assembly assembly)
        {
            foreach (Type type in assembly.GetExportedTypes())
            {
                if (!type.IsGenericType && typeof(IMapping).IsAssignableFrom(type))
                {
                    IMapping mapping = (IMapping)type.InstantiateUsingParameterlessConstructor();
                    AddMapping(mapping);
                }
            }
        }

        public void AddMapping(IMapping mapping)
        {
            _mappings.Add(mapping);
        }

        public Conventions Conventions
        {
            get { return _conventions; }
            protected set { _conventions = value; }
        }

        private static Assembly findTheCallingAssembly()
        {
            StackTrace trace = new StackTrace(Thread.CurrentThread, false);

            Assembly thisAssembly = Assembly.GetExecutingAssembly();
            Assembly callingAssembly = null;
            for (int i = 0; i < trace.FrameCount; i++)
            {
                StackFrame frame = trace.GetFrame(i);
                Assembly assembly = frame.GetMethod().DeclaringType.Assembly;
                if (assembly != thisAssembly)
                {
                    callingAssembly = assembly;
                    break;
                }
            }
            return callingAssembly;
        }

        public virtual void Configure(Configuration configuration)
        {
            _configured = true;

            MappingVisitor visitor = new MappingVisitor(_conventions, configuration);
            _mappings.ForEach(mapping => mapping.ApplyMappings(visitor));
        }

        public void WriteMappingsTo(string folder)
        {
            DiagnosticMappingVisitor visitor = new DiagnosticMappingVisitor(folder, _conventions, null);
            _mappings.ForEach(m => m.ApplyMappings(visitor));
        }
    }

    public class DiagnosticMappingVisitor : MappingVisitor
    {
        private string _folder;

        public DiagnosticMappingVisitor(string folder, Conventions conventions, Configuration configuration) : base(conventions, configuration)
        {
            _folder = folder;            
        }

        public override void AddMappingDocument(XmlDocument document, Type type)
        {
            string filename = Path.Combine(_folder, type.FullName + ".hbm.xml");
            document.Save(filename);
        }
    }
}