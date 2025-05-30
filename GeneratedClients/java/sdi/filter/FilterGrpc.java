package sdi.filter;

import static io.grpc.MethodDescriptor.generateFullMethodName;

/**
 */
@javax.annotation.Generated(
    value = "by gRPC proto compiler (version 1.58.0)",
    comments = "Source: filter.proto")
@io.grpc.stub.annotations.GrpcGenerated
public final class FilterGrpc {

  private FilterGrpc() {}

  public static final java.lang.String SERVICE_NAME = "sdi.filter.Filter";

  // Static method descriptors that strictly reflect the proto.
  private static volatile io.grpc.MethodDescriptor<sdi.filter.FilterOuterClass.FilterRequest,
      sdi.filter.FilterOuterClass.FilterReply> getFilterCallMethod;

  @io.grpc.stub.annotations.RpcMethod(
      fullMethodName = SERVICE_NAME + '/' + "FilterCall",
      requestType = sdi.filter.FilterOuterClass.FilterRequest.class,
      responseType = sdi.filter.FilterOuterClass.FilterReply.class,
      methodType = io.grpc.MethodDescriptor.MethodType.UNARY)
  public static io.grpc.MethodDescriptor<sdi.filter.FilterOuterClass.FilterRequest,
      sdi.filter.FilterOuterClass.FilterReply> getFilterCallMethod() {
    io.grpc.MethodDescriptor<sdi.filter.FilterOuterClass.FilterRequest, sdi.filter.FilterOuterClass.FilterReply> getFilterCallMethod;
    if ((getFilterCallMethod = FilterGrpc.getFilterCallMethod) == null) {
      synchronized (FilterGrpc.class) {
        if ((getFilterCallMethod = FilterGrpc.getFilterCallMethod) == null) {
          FilterGrpc.getFilterCallMethod = getFilterCallMethod =
              io.grpc.MethodDescriptor.<sdi.filter.FilterOuterClass.FilterRequest, sdi.filter.FilterOuterClass.FilterReply>newBuilder()
              .setType(io.grpc.MethodDescriptor.MethodType.UNARY)
              .setFullMethodName(generateFullMethodName(SERVICE_NAME, "FilterCall"))
              .setSampledToLocalTracing(true)
              .setRequestMarshaller(io.grpc.protobuf.ProtoUtils.marshaller(
                  sdi.filter.FilterOuterClass.FilterRequest.getDefaultInstance()))
              .setResponseMarshaller(io.grpc.protobuf.ProtoUtils.marshaller(
                  sdi.filter.FilterOuterClass.FilterReply.getDefaultInstance()))
              .setSchemaDescriptor(new FilterMethodDescriptorSupplier("FilterCall"))
              .build();
        }
      }
    }
    return getFilterCallMethod;
  }

  /**
   * Creates a new async stub that supports all call types for the service
   */
  public static FilterStub newStub(io.grpc.Channel channel) {
    io.grpc.stub.AbstractStub.StubFactory<FilterStub> factory =
      new io.grpc.stub.AbstractStub.StubFactory<FilterStub>() {
        @java.lang.Override
        public FilterStub newStub(io.grpc.Channel channel, io.grpc.CallOptions callOptions) {
          return new FilterStub(channel, callOptions);
        }
      };
    return FilterStub.newStub(factory, channel);
  }

  /**
   * Creates a new blocking-style stub that supports unary and streaming output calls on the service
   */
  public static FilterBlockingStub newBlockingStub(
      io.grpc.Channel channel) {
    io.grpc.stub.AbstractStub.StubFactory<FilterBlockingStub> factory =
      new io.grpc.stub.AbstractStub.StubFactory<FilterBlockingStub>() {
        @java.lang.Override
        public FilterBlockingStub newStub(io.grpc.Channel channel, io.grpc.CallOptions callOptions) {
          return new FilterBlockingStub(channel, callOptions);
        }
      };
    return FilterBlockingStub.newStub(factory, channel);
  }

  /**
   * Creates a new ListenableFuture-style stub that supports unary calls on the service
   */
  public static FilterFutureStub newFutureStub(
      io.grpc.Channel channel) {
    io.grpc.stub.AbstractStub.StubFactory<FilterFutureStub> factory =
      new io.grpc.stub.AbstractStub.StubFactory<FilterFutureStub>() {
        @java.lang.Override
        public FilterFutureStub newStub(io.grpc.Channel channel, io.grpc.CallOptions callOptions) {
          return new FilterFutureStub(channel, callOptions);
        }
      };
    return FilterFutureStub.newStub(factory, channel);
  }

  /**
   */
  public interface AsyncService {

    /**
     */
    default void filterCall(sdi.filter.FilterOuterClass.FilterRequest request,
        io.grpc.stub.StreamObserver<sdi.filter.FilterOuterClass.FilterReply> responseObserver) {
      io.grpc.stub.ServerCalls.asyncUnimplementedUnaryCall(getFilterCallMethod(), responseObserver);
    }
  }

  /**
   * Base class for the server implementation of the service Filter.
   */
  public static abstract class FilterImplBase
      implements io.grpc.BindableService, AsyncService {

    @java.lang.Override public final io.grpc.ServerServiceDefinition bindService() {
      return FilterGrpc.bindService(this);
    }
  }

  /**
   * A stub to allow clients to do asynchronous rpc calls to service Filter.
   */
  public static final class FilterStub
      extends io.grpc.stub.AbstractAsyncStub<FilterStub> {
    private FilterStub(
        io.grpc.Channel channel, io.grpc.CallOptions callOptions) {
      super(channel, callOptions);
    }

    @java.lang.Override
    protected FilterStub build(
        io.grpc.Channel channel, io.grpc.CallOptions callOptions) {
      return new FilterStub(channel, callOptions);
    }

    /**
     */
    public void filterCall(sdi.filter.FilterOuterClass.FilterRequest request,
        io.grpc.stub.StreamObserver<sdi.filter.FilterOuterClass.FilterReply> responseObserver) {
      io.grpc.stub.ClientCalls.asyncUnaryCall(
          getChannel().newCall(getFilterCallMethod(), getCallOptions()), request, responseObserver);
    }
  }

  /**
   * A stub to allow clients to do synchronous rpc calls to service Filter.
   */
  public static final class FilterBlockingStub
      extends io.grpc.stub.AbstractBlockingStub<FilterBlockingStub> {
    private FilterBlockingStub(
        io.grpc.Channel channel, io.grpc.CallOptions callOptions) {
      super(channel, callOptions);
    }

    @java.lang.Override
    protected FilterBlockingStub build(
        io.grpc.Channel channel, io.grpc.CallOptions callOptions) {
      return new FilterBlockingStub(channel, callOptions);
    }

    /**
     */
    public sdi.filter.FilterOuterClass.FilterReply filterCall(sdi.filter.FilterOuterClass.FilterRequest request) {
      return io.grpc.stub.ClientCalls.blockingUnaryCall(
          getChannel(), getFilterCallMethod(), getCallOptions(), request);
    }
  }

  /**
   * A stub to allow clients to do ListenableFuture-style rpc calls to service Filter.
   */
  public static final class FilterFutureStub
      extends io.grpc.stub.AbstractFutureStub<FilterFutureStub> {
    private FilterFutureStub(
        io.grpc.Channel channel, io.grpc.CallOptions callOptions) {
      super(channel, callOptions);
    }

    @java.lang.Override
    protected FilterFutureStub build(
        io.grpc.Channel channel, io.grpc.CallOptions callOptions) {
      return new FilterFutureStub(channel, callOptions);
    }

    /**
     */
    public com.google.common.util.concurrent.ListenableFuture<sdi.filter.FilterOuterClass.FilterReply> filterCall(
        sdi.filter.FilterOuterClass.FilterRequest request) {
      return io.grpc.stub.ClientCalls.futureUnaryCall(
          getChannel().newCall(getFilterCallMethod(), getCallOptions()), request);
    }
  }

  private static final int METHODID_FILTER_CALL = 0;

  private static final class MethodHandlers<Req, Resp> implements
      io.grpc.stub.ServerCalls.UnaryMethod<Req, Resp>,
      io.grpc.stub.ServerCalls.ServerStreamingMethod<Req, Resp>,
      io.grpc.stub.ServerCalls.ClientStreamingMethod<Req, Resp>,
      io.grpc.stub.ServerCalls.BidiStreamingMethod<Req, Resp> {
    private final AsyncService serviceImpl;
    private final int methodId;

    MethodHandlers(AsyncService serviceImpl, int methodId) {
      this.serviceImpl = serviceImpl;
      this.methodId = methodId;
    }

    @java.lang.Override
    @java.lang.SuppressWarnings("unchecked")
    public void invoke(Req request, io.grpc.stub.StreamObserver<Resp> responseObserver) {
      switch (methodId) {
        case METHODID_FILTER_CALL:
          serviceImpl.filterCall((sdi.filter.FilterOuterClass.FilterRequest) request,
              (io.grpc.stub.StreamObserver<sdi.filter.FilterOuterClass.FilterReply>) responseObserver);
          break;
        default:
          throw new AssertionError();
      }
    }

    @java.lang.Override
    @java.lang.SuppressWarnings("unchecked")
    public io.grpc.stub.StreamObserver<Req> invoke(
        io.grpc.stub.StreamObserver<Resp> responseObserver) {
      switch (methodId) {
        default:
          throw new AssertionError();
      }
    }
  }

  public static final io.grpc.ServerServiceDefinition bindService(AsyncService service) {
    return io.grpc.ServerServiceDefinition.builder(getServiceDescriptor())
        .addMethod(
          getFilterCallMethod(),
          io.grpc.stub.ServerCalls.asyncUnaryCall(
            new MethodHandlers<
              sdi.filter.FilterOuterClass.FilterRequest,
              sdi.filter.FilterOuterClass.FilterReply>(
                service, METHODID_FILTER_CALL)))
        .build();
  }

  private static abstract class FilterBaseDescriptorSupplier
      implements io.grpc.protobuf.ProtoFileDescriptorSupplier, io.grpc.protobuf.ProtoServiceDescriptorSupplier {
    FilterBaseDescriptorSupplier() {}

    @java.lang.Override
    public com.google.protobuf.Descriptors.FileDescriptor getFileDescriptor() {
      return sdi.filter.FilterOuterClass.getDescriptor();
    }

    @java.lang.Override
    public com.google.protobuf.Descriptors.ServiceDescriptor getServiceDescriptor() {
      return getFileDescriptor().findServiceByName("Filter");
    }
  }

  private static final class FilterFileDescriptorSupplier
      extends FilterBaseDescriptorSupplier {
    FilterFileDescriptorSupplier() {}
  }

  private static final class FilterMethodDescriptorSupplier
      extends FilterBaseDescriptorSupplier
      implements io.grpc.protobuf.ProtoMethodDescriptorSupplier {
    private final java.lang.String methodName;

    FilterMethodDescriptorSupplier(java.lang.String methodName) {
      this.methodName = methodName;
    }

    @java.lang.Override
    public com.google.protobuf.Descriptors.MethodDescriptor getMethodDescriptor() {
      return getServiceDescriptor().findMethodByName(methodName);
    }
  }

  private static volatile io.grpc.ServiceDescriptor serviceDescriptor;

  public static io.grpc.ServiceDescriptor getServiceDescriptor() {
    io.grpc.ServiceDescriptor result = serviceDescriptor;
    if (result == null) {
      synchronized (FilterGrpc.class) {
        result = serviceDescriptor;
        if (result == null) {
          serviceDescriptor = result = io.grpc.ServiceDescriptor.newBuilder(SERVICE_NAME)
              .setSchemaDescriptor(new FilterFileDescriptorSupplier())
              .addMethod(getFilterCallMethod())
              .build();
        }
      }
    }
    return result;
  }
}
