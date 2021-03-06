﻿
//===============================================================================
//
//  IMPORTANT NOTICE, PLEASE READ CAREFULLY:
//
//  ● This code is dual-licensed (GPLv3 + Commercial). Commercial licenses can be obtained from: http://cshtml5.com
//
//  ● You are NOT allowed to:
//       – Use this code in a proprietary or closed-source project (unless you have obtained a commercial license)
//       – Mix this code with non-GPL-licensed code (such as MIT-licensed code), or distribute it under a different license
//       – Remove or modify this notice
//
//  ● Copyright 2019 Userware/CSHTML5. This code is part of the CSHTML5 product.
//
//===============================================================================


#if WCF_STACK

#if UNIMPLEMENTED_MEMBERS
namespace System.ServiceModel
{
    public sealed class OperationContextScope : IDisposable
    {
        private readonly OperationContext _originalContext = OperationContext.Current;
        private readonly OperationContextScope _originalScope = _currentScope;

        [ThreadStatic]
        private static OperationContextScope _currentScope;

        private OperationContext _currentContext;
        private bool _disposed;

        /// <summary>
        /// Initialise une nouvelle instance de la classe <see cref="T:System.ServiceModel.OperationContextScope"/> qui utilise le <see cref="T:System.ServiceModel.IContextChannel"/> spécifié pour créer un <see cref="T:System.ServiceModel.OperationContext"/> pour la portée.
        /// </summary>
        /// <param name="channel">Le canal à utiliser lors de la création de la portée pour un nouveau <see cref="T:System.ServiceModel.OperationContext"/>.</param>
        public OperationContextScope(IContextChannel channel)
        {
            PushContext(new OperationContext(channel));
        }

        /// <summary>
        /// Initialise une nouvelle instance de la classe <see cref="T:System.ServiceModel.OperationContextScope"/> pour créer une portée pour l'objet <see cref="T:System.ServiceModel.OperationContext"/> spécifié.
        /// </summary>
        /// <param name="context">Le <see cref="T:System.ServiceModel.OperationContext"/> actif dans la portée créée.</param>
        public OperationContextScope(OperationContext context)
        {
            PushContext(context);
        }

        /// <summary>
        /// Rétablit le <see cref="T:System.ServiceModel.OperationContext"/> d'origine comme contexte actif et recycle l'objet <see cref="T:System.ServiceModel.OperationContextScope"/>.
        /// </summary>
        public void Dispose()
        {
            if (_disposed)
                return;
            _disposed = true;
            PopContext();
        }

        private void PushContext(OperationContext context)
        {
            _currentContext = context;
            _currentScope = this;
            OperationContext.Current = _currentContext;
        }

        private void PopContext()
        {
            _currentScope = _originalScope;
            OperationContext.Current = _originalContext;
        }
    }
}
#endif

#endif