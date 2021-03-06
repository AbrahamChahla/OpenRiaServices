﻿using System.Collections;
using System.Collections.Generic;
using System.ServiceModel;
using OpenRiaServices;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using OpenRiaServices.DomainServices.Hosting;
using System.Xml;

namespace OpenRiaServices.DomainServices.Hosting
{
    /// <summary>
    /// A SOAP endpoint behavior which injects a message inspector that parses query headers.
    /// </summary>
    public class SoapQueryBehavior : IEndpointBehavior
    {
        /// <summary>
        /// Implement to pass data at runtime to bindings to support custom behavior.
        /// </summary>
        /// <param name="endpoint">The endpoint to modify.</param>
        /// <param name="bindingParameters">The objects that binding elements require to support the behavior.</param>
        public void AddBindingParameters(ServiceEndpoint endpoint, BindingParameterCollection bindingParameters)
        {
        }

        /// <summary>
        /// Implements a modification or extension of the client across an endpoint.
        /// </summary>
        /// <param name="endpoint">The endpoint that is to be customized.</param>
        /// <param name="clientRuntime">The client runtime to be customized.</param>
        public void ApplyClientBehavior(ServiceEndpoint endpoint, ClientRuntime clientRuntime)
        {
        }

        /// <summary>
        /// Implements a modification or extension of the service across an endpoint.
        /// </summary>
        /// <param name="endpoint">The endpoint that exposes the contract.</param>
        /// <param name="endpointDispatcher">The endpoint dispatcher to be modified or extended.</param>
        public void ApplyDispatchBehavior(ServiceEndpoint endpoint, EndpointDispatcher endpointDispatcher)
        {
            endpointDispatcher.DispatchRuntime.MessageInspectors.Add(new SoapQueryInspector());
        }

        /// <summary>
        /// Implement to confirm that the endpoint meets some intended criteria.
        /// </summary>
        /// <param name="endpoint">The endpoint to validate.</param>
        public void Validate(ServiceEndpoint endpoint)
        {
        }

        private class SoapQueryInspector : IDispatchMessageInspector
        {
            private const string QueryPropertyName = "DomainServiceQuery";
            private const string QueryHeaderName = "DomainServiceQuery";
            private const string QueryHeaderNamespace = "DomainServices";

            public object AfterReceiveRequest(ref Message request, IClientChannel channel, InstanceContext instanceContext)
            {
                int headerPosition = request.Headers.FindHeader(QueryHeaderName, QueryHeaderNamespace);
                if (headerPosition > -1)
                {
                    XmlDictionaryReader reader = request.Headers.GetReaderAtHeader(headerPosition);
                    ServiceQuery serviceQuery = MessageUtility.ReadServiceQuery(reader);
                    request.Properties[QueryPropertyName] = serviceQuery;
                }

                return null;
            }

            public void BeforeSendReply(ref Message reply, object correlationState)
            {
            }
        }
    }
}