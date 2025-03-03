package sdi.parser;

import static io.grpc.MethodDescriptor.generateFullMethodName;

/**
 */
@javax.annotation.Generated(
    value = "by gRPC proto compiler (version 1.58.0)",
    comments = "Source: parser.proto")
@io.grpc.stub.annotations.GrpcGenerated
public final class ParserGrpc {

  private ParserGrpc() {}

  public static final java.lang.String SERVICE_NAME = "sdi.parser.Parser";

  // Static method descriptors that strictly reflect the proto.
  private static volatile io.grpc.MethodDescriptor<sdi.parser.ParserOuterClass.ParseRequest,
      sdi.parser.ParserOuterClass.ParseResponse> getParseCallMethod;

  @io.grpc.stub.annotations.RpcMethod(
      fullMethodName = SERVICE_NAME + '/' + "ParseCall",
      requestType = sdi.parser.ParserOuterClass.ParseRequest.class,
      responseType = sdi.parser.ParserOuterClass.ParseResponse.class,
      methodType = io.grpc.MethodDescriptor.MethodType.UNARY)
  public static io.grpc.MethodDescriptor<sdi.parser.ParserOuterClass.ParseRequest,
      sdi.parser.ParserOuterClass.ParseResponse> getParseCallMethod() {
    io.grpc.MethodDescriptor<sdi.parser.ParserOuterClass.ParseRequest, sdi.parser.ParserOuterClass.ParseResponse> getParseCallMethod;
    if ((getParseCallMethod = ParserGrpc.getParseCallMethod) == null) {
      synchronized (ParserGrpc.class) {
        if ((getParseCallMethod = ParserGrpc.getParseCallMethod) == null) {
          ParserGrpc.getParseCallMethod = getParseCallMethod =
              io.grpc.MethodDescriptor.<sdi.parser.ParserOuterClass.ParseRequest, sdi.parser.ParserOuterClass.ParseResponse>newBuilder()
              .setType(io.grpc.MethodDescriptor.MethodType.UNARY)
              .setFullMethodName(generateFullMethodName(SERVICE_NAME, "ParseCall"))
              .setSampledToLocalTracing(true)
              .setRequestMarshaller(io.grpc.protobuf.ProtoUtils.marshaller(
                  sdi.parser.ParserOuterClass.ParseRequest.getDefaultInstance()))
              .setResponseMarshaller(io.grpc.protobuf.ProtoUtils.marshaller(
                  sdi.parser.ParserOuterClass.ParseResponse.getDefaultInstance()))
              .setSchemaDescriptor(new ParserMethodDescriptorSupplier("ParseCall"))
              .build();
        }
      }
    }
    return getParseCallMethod;
  }

  /**
   * Creates a new async stub that supports all call types for the service
   */
  public static ParserStub newStub(io.grpc.Channel channel) {
    io.grpc.stub.AbstractStub.StubFactory<ParserStub> factory =
      new io.grpc.stub.AbstractStub.StubFactory<ParserStub>() {
        @java.lang.Override
        public ParserStub newStub(io.grpc.Channel channel, io.grpc.CallOptions callOptions) {
          return new ParserStub(channel, callOptions);
        }
      };
    return ParserStub.newStub(factory, channel);
  }

  /**
   * Creates a new blocking-style stub that supports unary and streaming output calls on the service
   */
  public static ParserBlockingStub newBlockingStub(
      io.grpc.Channel channel) {
    io.grpc.stub.AbstractStub.StubFactory<ParserBlockingStub> factory =
      new io.grpc.stub.AbstractStub.StubFactory<ParserBlockingStub>() {
        @java.lang.Override
        public ParserBlockingStub newStub(io.grpc.Channel channel, io.grpc.CallOptions callOptions) {
          return new ParserBlockingStub(channel, callOptions);
        }
      };
    return ParserBlockingStub.newStub(factory, channel);
  }

  /**
   * Creates a new ListenableFuture-style stub that supports unary calls on the service
   */
  public static ParserFutureStub newFutureStub(
      io.grpc.Channel channel) {
    io.grpc.stub.AbstractStub.StubFactory<ParserFutureStub> factory =
      new io.grpc.stub.AbstractStub.StubFactory<ParserFutureStub>() {
        @java.lang.Override
        public ParserFutureStub newStub(io.grpc.Channel channel, io.grpc.CallOptions callOptions) {
          return new ParserFutureStub(channel, callOptions);
        }
      };
    return ParserFutureStub.newStub(factory, channel);
  }

  /**
   */
  public interface AsyncService {

    /**
     */
    default void parseCall(sdi.parser.ParserOuterClass.ParseRequest request,
        io.grpc.stub.StreamObserver<sdi.parser.ParserOuterClass.ParseResponse> responseObserver) {
      io.grpc.stub.ServerCalls.asyncUnimplementedUnaryCall(getParseCallMethod(), responseObserver);
    }
  }

  /**
   * Base class for the server implementation of the service Parser.
   */
  public static abstract class ParserImplBase
      implements io.grpc.BindableService, AsyncService {

    @java.lang.Override public final io.grpc.ServerServiceDefinition bindService() {
      return ParserGrpc.bindService(this);
    }
  }

  /**
   * A stub to allow clients to do asynchronous rpc calls to service Parser.
   */
  public static final class ParserStub
      extends io.grpc.stub.AbstractAsyncStub<ParserStub> {
    private ParserStub(
        io.grpc.Channel channel, io.grpc.CallOptions callOptions) {
      super(channel, callOptions);
    }

    @java.lang.Override
    protected ParserStub build(
        io.grpc.Channel channel, io.grpc.CallOptions callOptions) {
      return new ParserStub(channel, callOptions);
    }

    /**
     */
    public void parseCall(sdi.parser.ParserOuterClass.ParseRequest request,
        io.grpc.stub.StreamObserver<sdi.parser.ParserOuterClass.ParseResponse> responseObserver) {
      io.grpc.stub.ClientCalls.asyncUnaryCall(
          getChannel().newCall(getParseCallMethod(), getCallOptions()), request, responseObserver);
    }
  }

  /**
   * A stub to allow clients to do synchronous rpc calls to service Parser.
   */
  public static final class ParserBlockingStub
      extends io.grpc.stub.AbstractBlockingStub<ParserBlockingStub> {
    private ParserBlockingStub(
        io.grpc.Channel channel, io.grpc.CallOptions callOptions) {
      super(channel, callOptions);
    }

    @java.lang.Override
    protected ParserBlockingStub build(
        io.grpc.Channel channel, io.grpc.CallOptions callOptions) {
      return new ParserBlockingStub(channel, callOptions);
    }

    /**
     */
    public sdi.parser.ParserOuterClass.ParseResponse parseCall(sdi.parser.ParserOuterClass.ParseRequest request) {
      return io.grpc.stub.ClientCalls.blockingUnaryCall(
          getChannel(), getParseCallMethod(), getCallOptions(), request);
    }
  }

  /**
   * A stub to allow clients to do ListenableFuture-style rpc calls to service Parser.
   */
  public static final class ParserFutureStub
      extends io.grpc.stub.AbstractFutureStub<ParserFutureStub> {
    private ParserFutureStub(
        io.grpc.Channel channel, io.grpc.CallOptions callOptions) {
      super(channel, callOptions);
    }

    @java.lang.Override
    protected ParserFutureStub build(
        io.grpc.Channel channel, io.grpc.CallOptions callOptions) {
      return new ParserFutureStub(channel, callOptions);
    }

    /**
     */
    public com.google.common.util.concurrent.ListenableFuture<sdi.parser.ParserOuterClass.ParseResponse> parseCall(
        sdi.parser.ParserOuterClass.ParseRequest request) {
      return io.grpc.stub.ClientCalls.futureUnaryCall(
          getChannel().newCall(getParseCallMethod(), getCallOptions()), request);
    }
  }

  private static final int METHODID_PARSE_CALL = 0;

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
        case METHODID_PARSE_CALL:
          serviceImpl.parseCall((sdi.parser.ParserOuterClass.ParseRequest) request,
              (io.grpc.stub.StreamObserver<sdi.parser.ParserOuterClass.ParseResponse>) responseObserver);
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
          getParseCallMethod(),
          io.grpc.stub.ServerCalls.asyncUnaryCall(
            new MethodHandlers<
              sdi.parser.ParserOuterClass.ParseRequest,
              sdi.parser.ParserOuterClass.ParseResponse>(
                service, METHODID_PARSE_CALL)))
        .build();
  }

  private static abstract class ParserBaseDescriptorSupplier
      implements io.grpc.protobuf.ProtoFileDescriptorSupplier, io.grpc.protobuf.ProtoServiceDescriptorSupplier {
    ParserBaseDescriptorSupplier() {}

    @java.lang.Override
    public com.google.protobuf.Descriptors.FileDescriptor getFileDescriptor() {
      return sdi.parser.ParserOuterClass.getDescriptor();
    }

    @java.lang.Override
    public com.google.protobuf.Descriptors.ServiceDescriptor getServiceDescriptor() {
      return getFileDescriptor().findServiceByName("Parser");
    }
  }

  private static final class ParserFileDescriptorSupplier
      extends ParserBaseDescriptorSupplier {
    ParserFileDescriptorSupplier() {}
  }

  private static final class ParserMethodDescriptorSupplier
      extends ParserBaseDescriptorSupplier
      implements io.grpc.protobuf.ProtoMethodDescriptorSupplier {
    private final java.lang.String methodName;

    ParserMethodDescriptorSupplier(java.lang.String methodName) {
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
      synchronized (ParserGrpc.class) {
        result = serviceDescriptor;
        if (result == null) {
          serviceDescriptor = result = io.grpc.ServiceDescriptor.newBuilder(SERVICE_NAME)
              .setSchemaDescriptor(new ParserFileDescriptorSupplier())
              .addMethod(getParseCallMethod())
              .build();
        }
      }
    }
    return result;
  }
}
