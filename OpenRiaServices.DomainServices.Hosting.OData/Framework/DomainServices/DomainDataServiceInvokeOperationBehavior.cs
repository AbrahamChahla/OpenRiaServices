using System;

namespace OpenRiaServices.DomainServices.Hosting.OData
{
    #region Namespace
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Linq;
    using System.ServiceModel.Description;
    using OpenRiaServices.DomainServices.Server;

    #endregion

    /// <summary>
    /// Operation behavior that applies InvokeOperationInvoker to
    /// all the [Invoke] attributed operations.
    /// </summary>
    internal class DomainDataServiceInvokeOperationBehavior : IOperationBehavior
    {
        /// <summary>Operation to which the behavior is applied.</summary>
        private DomainOperationEntry operation;

        /// <summary>Constructs a new instance of the behavior.</summary>
        /// <param name="operation">Operation to which the behavior is applied.</param>
        public DomainDataServiceInvokeOperationBehavior(DomainOperationEntry operation)
        {
            this.operation = operation;
        }

        /// <summary>
        /// Implement to pass data at runtime to bindings to support custom behavior.
        /// </summary>
        /// <param name="operationDescription">The operation being examined. Use for examination only. If the operation description is modified, the results are undefined.</param>
        /// <param name="bindingParameters">The collection of objects that binding elements require to support the behavior.</param>
        public void AddBindingParameters(OperationDescription operationDescription, System.ServiceModel.Channels.BindingParameterCollection bindingParameters)
        {
        }

        /// <summary>
        /// Implements a modification or extension of the client across an operation.
        /// </summary>
        /// <param name="operationDescription">The operation being examined. Use for examination only. If the operation description is modified, the results are undefined.</param>
        /// <param name="clientOperation">The run-time object that exposes customization properties for the operation described by operationDescription. </param>
        public void ApplyClientBehavior(OperationDescription operationDescription, System.ServiceModel.Dispatcher.ClientOperation clientOperation)
        {
        }

        /// <summary>
        /// Implements a modification or extension of the service across an operation.
        /// </summary>
        /// <param name="operationDescription">The operation being examined. Use for examination only. If the operation description is modified, the results are undefined.</param>
        /// <param name="dispatchOperation">The run-time object that exposes customization properties for the operation described by operationDescription.</param>
        public void ApplyDispatchBehavior(OperationDescription operationDescription, System.ServiceModel.Dispatcher.DispatchOperation dispatchOperation)
        {
                dispatchOperation.Invoker = new DomainDataServiceInvokeOperationInvoker(this.operation);
        }

        /// <summary>
        /// Implement to confirm that the operation meets some intended criteria.
        /// </summary>
        /// <param name="operationDescription">The operation being examined. Use for examination only. If the operation description is modified, the results are undefined.</param>
        public void Validate(OperationDescription operationDescription)
        {
        }

        /// <summary>Invoker for invoke operations.</summary>
        private class DomainDataServiceInvokeOperationInvoker : DomainDataServiceOperationInvoker
        {
            /// <summary>Operation to be invoked on the DomainService.</summary>
            private DomainOperationEntry operation;

            /// <summary>Create a new instance.</summary>
            /// <param name="operation">Operation to be invoked by this invoker.</param>
            public DomainDataServiceInvokeOperationInvoker(DomainOperationEntry operation)
                : base(DomainOperationType.Invoke)
            {
                this.operation = operation;
            }

            /// <summary>
            /// Returns an array of parameter objects.
            /// </summary>
            /// <returns>The parameters that are to be used as arguments to the operation.</returns>
            public override object[] AllocateInputs()
            {
                return new object[this.operation.Parameters.Count];
            }

            /// <summary>
            /// Derived classes override this method to provide custom invocation behavior.
            /// </summary>
            /// <param name="instance">Instance to invoke the invoker against.</param>
            /// <param name="inputs">Input parameters post conversion.</param>
            /// <param name="outputs">Optional out parameters.</param>
            /// <returns>Result of invocation.</returns>
            protected override object InvokeCore(object instance, object[] inputs, out object[] outputs)
            {
                outputs = ServiceUtils.EmptyObjectArray;

                IEnumerable<ValidationResult> validationErrors;
                object result;

                try
                {
                    InvokeDescription description = new InvokeDescription(this.operation, inputs);
                    result = ((DomainService)instance).Invoke(description, out validationErrors);
                }
                catch (UnauthorizedAccessException ex)
                {
                    throw new DomainDataServiceException((int)System.Net.HttpStatusCode.Unauthorized, ex.Message, ex);
                }
                catch (Exception ex)
                {
                    if (ex.IsFatal())
                    {
                        throw;
                    }
                    else
                    {
                        throw new DomainDataServiceException(Resource.DomainDataService_General_Error, ex);
                    }
                }

                DomainDataServiceException.HandleValidationErrors(validationErrors);

                return result;
            }

            /// <summary>
            /// Converts input parameters in place.
            /// </summary>
            /// <param name="inputs">Input parameters.</param>
            protected override void ConvertInputs(object[] inputs)
            {
                for (int i = 0; i < this.operation.Parameters.Count; i++)
                {
                    DomainOperationParameter parameter = this.operation.Parameters[i];
                    inputs[i] = TypeUtils.GetServerValue(parameter.ParameterType, inputs[i]);
                }
            }

            /// <summary>
            /// Converts the return value.
            /// </summary>
            /// <param name="returnValue">Return value.</param>
            /// <returns>Converted return value.</returns>
            protected override object ConvertReturnValue(object returnValue)
            {
                return TypeUtils.GetClientValue(TypeUtils.GetClientType(this.operation.ReturnType), returnValue);
            }
        }
    }
}

