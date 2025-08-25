using InsightDocs.TypeScript.Abstractions;
using InsightDocs.TypeScript.Model;
using InsightDocs.TypeScript.Model.Types;

namespace InsightDocs.TypeScript.Services
{
    public class MDNUrlResolver : IMDNUrlResolver
    {
        public static readonly Dictionary<string, string> MDNUrlMappings = new Dictionary<string, string>
        {
            { "https://developer.mozilla.org/docs/Web/API/HTMLElement/cancel_event", "https://developer.mozilla.org/docs/Web/API/HTMLElement" },
            { "https://developer.mozilla.org/docs/Web/API/HTMLDialogElement/cancel_event", "https://developer.mozilla.org/docs/Web/API/HTMLElement" },
            { "https://developer.mozilla.org/docs/Web/API/HTMLVideoElement/resize_event", "https://developer.mozilla.org/docs/Web/API/VisualViewport/resize_event" },
            { "https://developer.mozilla.org/docs/Web/API/HTMLTableRowElement/cells", "https://developer.mozilla.org/docs/Web/API/HTMLTableRowElement" },
            { "https://developer.mozilla.org/docs/Web/API/HTMLTableRowElement/sectionRowIndex", "https://developer.mozilla.org/docs/Web/API/HTMLTableRowElement" },
            { "https://developer.mozilla.org/docs/Web/API/HTMLTableCellElement/colSpan", "https://developer.mozilla.org/docs/Web/API/HTMLTableCellElement" },
            { "https://developer.mozilla.org/docs/Web/API/HTMLTableCellElement/cellIndex", "https://developer.mozilla.org/docs/Web/API/HTMLTableCellElement" },
            { "https://developer.mozilla.org/docs/Web/API/HTMLTableCellElement/abbr", "https://developer.mozilla.org/docs/Web/API/HTMLTableCellElement" },
            { "https://developer.mozilla.org/docs/Web/API/HTMLTableCellElement/rowSpan", "https://developer.mozilla.org/docs/Web/API/HTMLTableCellElement" },
            { "https://developer.mozilla.org/docs/Web/API/HTMLTableCellElement/headers", "https://developer.mozilla.org/docs/Web/API/HTMLTableCellElement" },
            { "https://developer.mozilla.org/docs/Web/API/HTMLTableCellElement/scope", "https://developer.mozilla.org/docs/Web/API/HTMLTableCellElement" },
            { "https://developer.mozilla.org/en-US/docs/Web/API/Plugin/item", "https://developer.mozilla.org/en-US/docs/Web/API/Plugin" },
            { "https://developer.mozilla.org/en-US/docs/Web/API/Plugin/namedItem", "https://developer.mozilla.org/en-US/docs/Web/API/Plugin" },
            { "https://developer.mozilla.org/docs/Web/API/Document/queryCommandValue", "" }
        };

        public static readonly Dictionary<string, string> TypeScriptTypes = new Dictionary<string, string>
        {
            { "Awaited", "awaitedtype" },
            { "NonNullable", "nonnullabletype" },
            { "Parameters", "parameterstype" },
            { "ConstructorParameters", "constructorparameterstype" },
            { "ReturnType", "returntypetype" },
            { "InstanceType", "instancetypetype" },
            { "ThisParameterType", "thisparametertype" },
            { "OmitThisParameter", "omitthisparametertype" },
            { "Required", "requiredtype" },
            { "Uppercase", "uppercasestringtype" },
            { "Lowercase", "lowercasestringtype" },
            { "Capitalize", "capitalizestringtype" },
            { "Uncapitalize", "uncapitalizestringtype" },
            { "Record", "recordkeys-type" },
            { "Partial", "partialtype" },
            { "Pick", "picktype-keys" },
            { "Exclude", "excludeuniontype-excludedmembers" },
            { "Readonly", "readonlytype" },
            { "Omit", "omittype-keys" },
            { "Extract", "extracttype-union" },
            { "ThisType<T>", "thistypetype" }
        };

        public static readonly Dictionary<string, string> JavaScriptTypeMappings = new Dictionary<string, string>
        {
            { "ElementCSSInlineStyle", "HTMLElement" },
            { "HTMLOrSVGElement", "SVGElement" },
            { "NonDocumentTypeChildNode", "Node" },
            { "DocumentAndElementEventHandlers", "HTMLElement" },
            { "DocumentAndElementEventHandlersEventMap", "Event" },
            { "GlobalEventHandlersEventMap", "Event" },
            { "InnerHTML", "Element" },
            { "ElementContentEditable", "HTMLElement" },
            { "Animatable", "Element" },
            { "DocumentEvent", "Event" },
            { "NodeListOf", "NodeList" },
            { "ObjectConstructor", "Object" },
            { "ActiveXObject", "Object" },
            { "ChildNode", "Node" },
            { "RegExpExecArray", "Array" },
            { "RegExpMatchArray", "Array" },
            { "ARIAMixin", "Element" },
            { "Slottable", "Element" },
            { "ParentNode", "Node" },
            { "PromiseLike", "Promise" },
            { "Iterable", "Iterator" },
            { "iterator", "Iterator" },
            { "ArrayLike", "Array" },
            { "PropertyKey", "String" },
            { "ElementEventMap", "Event" },
            { "HTMLElementEventMap", "Event" },
            { "HTMLCollectionOf", "NodeList" },
            { "CanvasRenderingContext2DSettings", "CanvasRenderingContext2D" },
            { "HTMLTableHeaderCellElement", "HTMLTableCellElement" },
            { "HTMLFrameElement", "HTMLElement" },
            { "DocumentOrShadowRoot", "Document" },
            { "FontFaceSource", "FontFace" },
            { "XPathEvaluatorBase", "XPathEvaluator" },
            { "IterableIterator", "Iterator" },
            { "EventListenerOrEventListenerObject", "EventTarget" },
            { "EventListener", "EventTarget" },
            { "AddEventListenerOptions", "EventTarget" },
            { "CustomElementConstructor", "HTMLElement" },
            { "CanvasLineJoin", "CanvasRenderingContext2D" },
            { "CanvasLineCap", "CanvasRenderingContext2D" },
            { "CanvasTextAlign", "CanvasRenderingContext2D" },
            { "CanvasTextBaseline", "CanvasRenderingContext2D" },
            { "NumberFormatOptions", "Intl.NumberFormat" },
            { "NonElementParentNode", "Node" }
        };

        public static readonly string[] JavaScriptGlobalObjects =
        [
            "Object",
            "Function",
            "Boolean",
            "Symbol",
            "Error",
            "AggregateError",
            "EvalError",
            "RangeError",
            "ReferenceError",
            "SyntaxError",
            "TypeError",
            "URIError",
            "InternalError",
            "Number",
            "BigInt",
            "Math",
            "Date",
            "String",
            "RegExp",
            "Array",
            "Int8Array",
            "Uint8Array",
            "Uint8ClampedArray",
            "Int16Array",
            "Uint16Array",
            "Int32Array",
            "Uint32Array",
            "BigInt64Array",
            "BigUint64Array",
            "Float32Array",
            "Float64Array",
            "Map",
            "Set",
            "WeakMap",
            "WeakSet",
            "ArrayBuffer",
            "SharedArrayBuffer",
            "DataView",
            "Atomics",
            "JSON",
            "WeakRef",
            "FinalizationRegistry",
            "Iterator",
            "AsyncIterator",
            "Promise",
            "GeneratorFunction",
            "AsyncGeneratorFunction",
            "Generator",
            "AsyncGenerator",
            "AsyncFunction",
            "Reflection",
            "Reflect",
            "Proxy",
            "Intl",
            "Intl.NumberFormat"
        ];

        public static readonly string[] JavaScriptBuiltinTypes =
        [
            "AbortController",
            "AbortSignal",
            "AbsoluteOrientationSensor",
            "AbstractRange",
            "Accelerometer",
            "AesCbcParams",
            "AesCtrParams",
            "AesGcmParams",
            "AesKeyGenParams",
            "AmbientLightSensor",
            "AnalyserNode",
            "Animation",
            "AnimationEffect",
            "AnimationEvent",
            "AnimationPlaybackEvent",
            "AnimationTimeline",
            "Attr",
            "AudioBuffer",
            "AudioBufferSourceNode",
            "AudioContext",
            "AudioData",
            "AudioDecoder",
            "AudioDestinationNode",
            "AudioEncoder",
            "AudioListener",
            "AudioNode",
            "AudioParam",
            "AudioParamDescriptor",
            "AudioParamMap",
            "AudioProcessingEvent",
            "AudioScheduledSourceNode",
            "AudioSinkInfo",
            "AudioTrack",
            "AudioTrackList",
            "AudioWorklet",
            "AudioWorkletGlobalScope",
            "AudioWorkletNode",
            "AudioWorkletProcessor",
            "AuthenticatorAssertionResponse",
            "AuthenticatorAttestationResponse",
            "AuthenticatorResponse",
            "BackgroundFetchEvent",
            "BackgroundFetchManager",
            "BackgroundFetchRecord",
            "BackgroundFetchRegistration",
            "BackgroundFetchUpdateUIEvent",
            "BarcodeDetector",
            "BarProp",
            "BaseAudioContext",
            "BatteryManager",
            "BeforeInstallPromptEvent",
            "BeforeUnloadEvent",
            "BiquadFilterNode",
            "Blob",
            "BlobEvent",
            "Bluetooth",
            "BluetoothCharacteristicProperties",
            "BluetoothDevice",
            "BluetoothRemoteGATTCharacteristic",
            "BluetoothRemoteGATTDescriptor",
            "BluetoothRemoteGATTServer",
            "BluetoothRemoteGATTService",
            "BluetoothUUID",
            "BroadcastChannel",
            "ByteLengthQueuingStrategy",
            "Cache",
            "CacheStorage",
            "CanMakePaymentEvent",
            "CanvasCaptureMediaStreamTrack",
            "CanvasGradient",
            "CanvasPattern",
            "CanvasRenderingContext2D",
            "CaptureController",
            "CaretPosition",
            "CDATASection",
            "ChannelMergerNode",
            "ChannelSplitterNode",
            "CharacterBoundsUpdateEvent",
            "CharacterData",
            "Client",
            "Clients",
            "Clipboard",
            "ClipboardEvent",
            "ClipboardItem",
            "CloseEvent",
            "Comment",
            "CompositionEvent",
            "CompressionStream",
            "console",
            "ConstantSourceNode",
            "ContactAddress",
            "ContactsManager",
            "ContentIndex",
            "ContentIndexEvent",
            "ContentVisibilityAutoStateChangeEvent",
            "ConvolverNode",
            "CookieChangeEvent",
            "CookieStore",
            "CookieStoreManager",
            "CountQueuingStrategy",
            "Credential",
            "CredentialsContainer",
            "Crypto",
            "CryptoKey",
            "CryptoKeyPair",
            "CSPViolationReportBody",
            "CSS",
            "CSSAnimation",
            "CSSConditionRule",
            "CSSContainerRule",
            "CSSCounterStyleRule",
            "CSSFontFaceRule",
            "CSSFontFeatureValuesRule",
            "CSSFontPaletteValuesRule",
            "CSSGroupingRule",
            "CSSImageValue",
            "CSSImportRule",
            "CSSKeyframeRule",
            "CSSKeyframesRule",
            "CSSKeywordValue",
            "CSSLayerBlockRule",
            "CSSLayerStatementRule",
            "CSSMathInvert",
            "CSSMathMax",
            "CSSMathMin",
            "CSSMathNegate",
            "CSSMathProduct",
            "CSSMathSum",
            "CSSMathValue",
            "CSSMatrixComponent",
            "CSSMediaRule",
            "CSSNamespaceRule",
            "CSSNumericArray",
            "CSSNumericValue",
            "CSSPageRule",
            "CSSPerspective",
            "CSSPositionValue",
            "CSSPrimitiveValue",
            "CSSPropertyRule",
            "CSSPseudoElement",
            "CSSRotate",
            "CSSRule",
            "CSSRuleList",
            "CSSScale",
            "CSSScopeRule",
            "CSSSkew",
            "CSSSkewX",
            "CSSSkewY",
            "CSSStartingStyleRule",
            "CSSStyleDeclaration",
            "CSSStyleRule",
            "CSSStyleSheet",
            "CSSStyleValue",
            "CSSSupportsRule",
            "CSSTransformComponent",
            "CSSTransformValue",
            "CSSTransition",
            "CSSTranslate",
            "CSSUnitValue",
            "CSSUnparsedValue",
            "CSSValue",
            "CSSValueList",
            "CSSVariableReferenceValue",
            "CustomElementRegistry",
            "CustomEvent",
            "CustomStateSet",
            "DataTransfer",
            "DataTransferItem",
            "DataTransferItemList",
            "DecompressionStream",
            "DedicatedWorkerGlobalScope",
            "DelayNode",
            "DeprecationReportBody",
            "DeviceMotionEvent",
            "DeviceMotionEventAcceleration",
            "DeviceMotionEventRotationRate",
            "DeviceOrientationEvent",
            "DirectoryEntrySync",
            "DirectoryReaderSync",
            "Document",
            "DocumentFragment",
            "DocumentPictureInPicture",
            "DocumentPictureInPictureEvent",
            "DocumentTimeline",
            "DocumentType",
            "DOMError",
            "DOMException",
            "DOMHighResTimeStamp",
            "DOMImplementation",
            "DOMMatrixReadOnly",
            "DOMParser",
            "DOMPoint",
            "DOMPointReadOnly",
            "DOMQuad",
            "DOMRect",
            "DOMRectReadOnly",
            "DOMStringList",
            "DOMStringMap",
            "DOMTokenList",
            "DragEvent",
            "DynamicsCompressorNode",
            "EcdhKeyDeriveParams",
            "EcdsaParams",
            "EcKeyGenParams",
            "EcKeyImportParams",
            "EditContext",
            "Element",
            "ElementInternals",
            "EncodedAudioChunk",
            "EncodedVideoChunk",
            "ErrorEvent",
            "Event",
            "EventCounts",
            "EventSource",
            "EventTarget",
            "ExtendableCookieChangeEvent",
            "ExtendableEvent",
            "ExtendableMessageEvent",
            "EyeDropper",
            "FeaturePolicy",
            "FederatedCredential",
            "Fence",
            "FencedFrameConfig",
            "FetchEvent",
            "File",
            "FileEntrySync",
            "FileList",
            "FileReader",
            "FileReaderSync",
            "FileSystem",
            "FileSystemDirectoryEntry",
            "FileSystemDirectoryHandle",
            "FileSystemDirectoryReader",
            "FileSystemEntry",
            "FileSystemFileEntry",
            "FileSystemFileHandle",
            "FileSystemHandle",
            "FileSystemSync",
            "FileSystemSyncAccessHandle",
            "FileSystemWritableFileStream",
            "FocusEvent",
            "FontData",
            "FontFace",
            "FontFaceSet",
            "FontFaceSetLoadEvent",
            "FormData",
            "FormDataEvent",
            "FragmentDirective",
            "GainNode",
            "Gamepad",
            "GamepadButton",
            "GamepadEvent",
            "GamepadHapticActuator",
            "GamepadPose",
            "Geolocation",
            "GeolocationCoordinates",
            "GeolocationPosition",
            "GeolocationPositionError",
            "GestureEvent",
            "GPU",
            "GPUAdapter",
            "GPUAdapterInfo",
            "GPUBindGroup",
            "GPUBindGroupLayout",
            "GPUBuffer",
            "GPUCanvasContext",
            "GPUCommandBuffer",
            "GPUCommandEncoder",
            "GPUCompilationInfo",
            "GPUCompilationMessage",
            "GPUComputePassEncoder",
            "GPUComputePipeline",
            "GPUDevice",
            "GPUDeviceLostInfo",
            "GPUError",
            "GPUExternalTexture",
            "GPUInternalError",
            "GPUOutOfMemoryError",
            "GPUPipelineError",
            "GPUPipelineLayout",
            "GPUQuerySet",
            "GPUQueue",
            "GPURenderBundle",
            "GPURenderBundleEncoder",
            "GPURenderPassEncoder",
            "GPURenderPipeline",
            "GPUSampler",
            "GPUShaderModule",
            "GPUSupportedFeatures",
            "GPUSupportedLimits",
            "GPUTexture",
            "GPUTextureView",
            "GPUUncapturedErrorEvent",
            "GPUValidationError",
            "GravitySensor",
            "Gyroscope",
            "HashChangeEvent",
            "Headers",
            "HID",
            "HIDConnectionEvent",
            "HIDDevice",
            "HIDInputReportEvent",
            "Highlight",
            "HighlightRegistry",
            "History",
            "HkdfParams",
            "HmacImportParams",
            "HmacKeyGenParams",
            "HMDVRDevice",
            "HTMLAllCollection",
            "HTMLAnchorElement",
            "HTMLAreaElement",
            "HTMLAudioElement",
            "HTMLBaseElement",
            "HTMLBodyElement",
            "HTMLBRElement",
            "HTMLButtonElement",
            "HTMLCanvasElement",
            "HTMLCollection",
            "HTMLDataElement",
            "HTMLDataListElement",
            "HTMLDetailsElement",
            "HTMLDialogElement",
            "HTMLDivElement",
            "HTMLDListElement",
            "HTMLDocument",
            "HTMLElement",
            "HTMLEmbedElement",
            "HTMLFencedFrameElement",
            "HTMLFieldSetElement",
            "HTMLFontElement",
            "HTMLFormControlsCollection",
            "HTMLFormElement",
            "HTMLFrameSetElement",
            "HTMLHeadElement",
            "HTMLHeadingElement",
            "HTMLHRElement",
            "HTMLHtmlElement",
            "HTMLIFrameElement",
            "HTMLImageElement",
            "HTMLInputElement",
            "HTMLLabelElement",
            "HTMLLegendElement",
            "HTMLLIElement",
            "HTMLLinkElement",
            "HTMLMapElement",
            "HTMLMarqueeElement",
            "HTMLMediaElement",
            "HTMLMenuElement",
            "HTMLMenuItemElement",
            "HTMLMetaElement",
            "HTMLMeterElement",
            "HTMLModElement",
            "HTMLObjectElement",
            "HTMLOListElement",
            "HTMLOptGroupElement",
            "HTMLOptionElement",
            "HTMLOptionsCollection",
            "HTMLOutputElement",
            "HTMLParagraphElement",
            "HTMLParamElement",
            "HTMLPictureElement",
            "HTMLPreElement",
            "HTMLProgressElement",
            "HTMLQuoteElement",
            "HTMLScriptElement",
            "HTMLSelectElement",
            "HTMLSlotElement",
            "HTMLSourceElement",
            "HTMLSpanElement",
            "HTMLStyleElement",
            "HTMLTableCaptionElement",
            "HTMLTableCellElement",
            "HTMLTableColElement",
            "HTMLTableElement",
            "HTMLTableRowElement",
            "HTMLTableSectionElement",
            "HTMLTemplateElement",
            "HTMLTextAreaElement",
            "HTMLTimeElement",
            "HTMLTitleElement",
            "HTMLTrackElement",
            "HTMLUListElement",
            "HTMLUnknownElement",
            "HTMLVideoElement",
            "IDBCursor",
            "IDBCursorWithValue",
            "IDBDatabase",
            "IDBFactory",
            "IDBIndex",
            "IDBKeyRange",
            "IDBObjectStore",
            "IDBOpenDBRequest",
            "IDBRequest",
            "IDBTransaction",
            "IDBVersionChangeEvent",
            "IdentityCredential",
            "IdentityProvider",
            "IdleDeadline",
            "IdleDetector",
            "IIRFilterNode",
            "ImageBitmap",
            "ImageBitmapRenderingContext",
            "ImageCapture",
            "ImageData",
            "ImageDecoder",
            "ImageTrack",
            "ImageTrackList",
            "Ink",
            "InkPresenter",
            "InputDeviceCapabilities",
            "InputDeviceInfo",
            "InputEvent",
            "InstallEvent",
            "IntersectionObserver",
            "IntersectionObserverEntry",
            "InterventionReportBody",
            "Keyboard",
            "KeyboardEvent",
            "KeyboardLayoutMap",
            "KeyframeEffect",
            "LargestContentfulPaint",
            "LaunchParams",
            "LaunchQueue",
            "LayoutShift",
            "LayoutShiftAttribution",
            "LinearAccelerationSensor",
            "Location",
            "Lock",
            "LockManager",
            "Magnetometer",
            "MathMLElement",
            "MediaCapabilities",
            "MediaDeviceInfo",
            "MediaDevices",
            "MediaElementAudioSourceNode",
            "MediaEncryptedEvent",
            "MediaError",
            "MediaKeyMessageEvent",
            "MediaKeys",
            "MediaKeySession",
            "MediaKeyStatusMap",
            "MediaKeySystemAccess",
            "MediaList",
            "MediaMetadata",
            "MediaQueryList",
            "MediaQueryListEvent",
            "MediaRecorder",
            "MediaRecorderErrorEvent",
            "MediaSession",
            "MediaSource",
            "MediaSourceHandle",
            "MediaStream",
            "MediaStreamAudioDestinationNode",
            "MediaStreamAudioSourceNode",
            "MediaStreamEvent",
            "MediaStreamTrack",
            "MediaStreamTrackAudioSourceNode",
            "MediaStreamTrackEvent",
            "MediaStreamTrackGenerator",
            "MediaStreamTrackProcessor",
            "MediaTrackConstraints",
            "MediaTrackSettings",
            "MediaTrackSupportedConstraints",
            "MerchantValidationEvent",
            "MessageChannel",
            "MessageEvent",
            "MessagePort",
            "Metadata",
            "MIDIAccess",
            "MIDIConnectionEvent",
            "MIDIInput",
            "MIDIInputMap",
            "MIDIMessageEvent",
            "MIDIOutput",
            "MIDIOutputMap",
            "MIDIPort",
            "MimeType",
            "MimeTypeArray",
            "MouseEvent",
            "MouseScrollEvent",
            "MutationEvent",
            "MutationObserver",
            "MutationRecord",
            "NamedNodeMap",
            "NavigateEvent",
            "Navigation",
            "NavigationCurrentEntryChangeEvent",
            "NavigationDestination",
            "NavigationHistoryEntry",
            "NavigationPreloadManager",
            "NavigationTransition",
            "Navigator",
            "NavigatorLogin",
            "NavigatorUAData",
            "NDEFMessage",
            "NDEFReader",
            "NDEFReadingEvent",
            "NDEFRecord",
            "NetworkInformation",
            "Node",
            "NodeIterator",
            "NodeList",
            "Notification",
            "NotificationEvent",
            "OfflineAudioCompletionEvent",
            "OfflineAudioContext",
            "OffscreenCanvas",
            "OffscreenCanvasRenderingContext2D",
            "OrientationSensor",
            "OscillatorNode",
            "OTPCredential",
            "OverconstrainedError",
            "PageTransitionEvent",
            "PaintWorkletGlobalScope",
            "PannerNode",
            "PasswordCredential",
            "Path2D",
            "PaymentAddress",
            "PaymentManager",
            "PaymentMethodChangeEvent",
            "PaymentRequest",
            "PaymentRequestEvent",
            "PaymentRequestUpdateEvent",
            "PaymentResponse",
            "Pbkdf2Params",
            "Performance",
            "PerformanceElementTiming",
            "PerformanceEntry",
            "PerformanceEventTiming",
            "PerformanceLongTaskTiming",
            "PerformanceMark",
            "PerformanceMeasure",
            "PerformanceNavigation",
            "PerformanceNavigationTiming",
            "PerformanceObserver",
            "PerformanceObserverEntryList",
            "PerformancePaintTiming",
            "PerformanceResourceTiming",
            "PerformanceServerTiming",
            "PerformanceTiming",
            "PeriodicSyncEvent",
            "PeriodicSyncManager",
            "PeriodicWave",
            "Permissions",
            "PermissionStatus",
            "PictureInPictureEvent",
            "PictureInPictureWindow",
            "Plugin",
            "PluginArray",
            "Point",
            "PointerEvent",
            "PopStateEvent",
            "PositionSensorVRDevice",
            "Presentation",
            "PresentationAvailability",
            "PresentationConnection",
            "PresentationConnectionAvailableEvent",
            "PresentationConnectionCloseEvent",
            "PresentationConnectionList",
            "PresentationReceiver",
            "PresentationRequest",
            "ProcessingInstruction",
            "ProgressEvent",
            "PromiseRejectionEvent",
            "PublicKeyCredential",
            "PushEvent",
            "PushManager",
            "PushMessageData",
            "PushSubscription",
            "PushSubscriptionOptions",
            "RadioNodeList",
            "Range",
            "ReadableByteStreamController",
            "ReadableStream",
            "ReadableStreamBYOBReader",
            "ReadableStreamBYOBRequest",
            "ReadableStreamDefaultController",
            "ReadableStreamDefaultReader",
            "RelativeOrientationSensor",
            "RemotePlayback",
            "Report",
            "ReportBody",
            "ReportingObserver",
            "Request",
            "RequestInit",
            "ResizeObserver",
            "ResizeObserverEntry",
            "ResizeObserverSize",
            "Response",
            "RsaHashedImportParams",
            "RsaHashedKeyGenParams",
            "RsaOaepParams",
            "RsaPssParams",
            "RTCAudioSourceStats",
            "RTCCertificate",
            "RTCCertificateStats",
            "RTCCodecStats",
            "RTCDataChannel",
            "RTCDataChannelEvent",
            "RTCDtlsTransport",
            "RTCDTMFSender",
            "RTCDTMFToneChangeEvent",
            "RTCEncodedAudioFrame",
            "RTCEncodedVideoFrame",
            "RTCError",
            "RTCErrorEvent",
            "RTCIceCandidate",
            "RTCIceCandidatePair",
            "RTCIceCandidatePairStats",
            "RTCIceCandidateStats",
            "RTCIceParameters",
            "RTCIceTransport",
            "RTCIdentityAssertion",
            "RTCInboundRtpStreamStats",
            "RTCOutboundRtpStreamStats",
            "RTCPeerConnection",
            "RTCPeerConnectionIceErrorEvent",
            "RTCPeerConnectionIceEvent",
            "RTCPeerConnectionStats",
            "RTCRemoteOutboundRtpStreamStats",
            "RTCRtpCodecParameters",
            "RTCRtpReceiver",
            "RTCRtpScriptTransform",
            "RTCRtpScriptTransformer",
            "RTCRtpSender",
            "RTCRtpStreamStats",
            "RTCRtpTransceiver",
            "RTCSctpTransport",
            "RTCSessionDescription",
            "RTCStatsReport",
            "RTCTrackEvent",
            "RTCTransformEvent",
            "RTCTransportStats",
            "Sanitizer",
            "Scheduler",
            "Scheduling",
            "Screen",
            "ScreenDetailed",
            "ScreenDetails",
            "ScreenOrientation",
            "ScriptProcessorNode",
            "ScrollTimeline",
            "SecurePaymentConfirmationRequest",
            "SecurityPolicyViolationEvent",
            "Selection",
            "Sensor",
            "SensorErrorEvent",
            "Serial",
            "SerialPort",
            "ServiceWorker",
            "ServiceWorkerContainer",
            "ServiceWorkerGlobalScope",
            "ServiceWorkerRegistration",
            "ShadowRoot",
            "SharedStorage",
            "SharedStorageOperation",
            "SharedStorageRunOperation",
            "SharedStorageSelectURLOperation",
            "SharedStorageWorklet",
            "SharedStorageWorkletGlobalScope",
            "SharedWorker",
            "SharedWorkerGlobalScope",
            "SourceBuffer",
            "SourceBufferList",
            "SpeechGrammar",
            "SpeechGrammarList",
            "SpeechRecognition",
            "SpeechRecognitionAlternative",
            "SpeechRecognitionErrorEvent",
            "SpeechRecognitionEvent",
            "SpeechRecognitionResult",
            "SpeechRecognitionResultList",
            "SpeechSynthesis",
            "SpeechSynthesisErrorEvent",
            "SpeechSynthesisEvent",
            "SpeechSynthesisUtterance",
            "SpeechSynthesisVoice",
            "StaticRange",
            "StereoPannerNode",
            "Storage",
            "StorageEvent",
            "StorageManager",
            "StylePropertyMap",
            "StylePropertyMapReadOnly",
            "StyleSheet",
            "StyleSheetList",
            "SubmitEvent",
            "SubtleCrypto",
            "SVGAElement",
            "SVGAngle",
            "SVGAnimateColorElement",
            "SVGAnimatedAngle",
            "SVGAnimatedBoolean",
            "SVGAnimatedEnumeration",
            "SVGAnimatedInteger",
            "SVGAnimatedLength",
            "SVGAnimatedLengthList",
            "SVGAnimatedNumber",
            "SVGAnimatedNumberList",
            "SVGAnimatedPreserveAspectRatio",
            "SVGAnimatedRect",
            "SVGAnimatedString",
            "SVGAnimatedTransformList",
            "SVGAnimateElement",
            "SVGAnimateMotionElement",
            "SVGAnimateTransformElement",
            "SVGAnimationElement",
            "SVGCircleElement",
            "SVGClipPathElement",
            "SVGComponentTransferFunctionElement",
            "SVGCursorElement",
            "SVGDefsElement",
            "SVGDescElement",
            "SVGElement",
            "SVGEllipseElement",
            "SVGEvent",
            "SVGFEBlendElement",
            "SVGFEColorMatrixElement",
            "SVGFEComponentTransferElement",
            "SVGFECompositeElement",
            "SVGFEConvolveMatrixElement",
            "SVGFEDiffuseLightingElement",
            "SVGFEDisplacementMapElement",
            "SVGFEDistantLightElement",
            "SVGFEDropShadowElement",
            "SVGFEFloodElement",
            "SVGFEFuncAElement",
            "SVGFEFuncBElement",
            "SVGFEFuncGElement",
            "SVGFEFuncRElement",
            "SVGFEGaussianBlurElement",
            "SVGFEImageElement",
            "SVGFEMergeElement",
            "SVGFEMergeNodeElement",
            "SVGFEMorphologyElement",
            "SVGFEOffsetElement",
            "SVGFEPointLightElement",
            "SVGFESpecularLightingElement",
            "SVGFESpotLightElement",
            "SVGFETileElement",
            "SVGFETurbulenceElement",
            "SVGFilterElement",
            "SVGFontElement",
            "SVGFontFaceElement",
            "SVGFontFaceFormatElement",
            "SVGFontFaceNameElement",
            "SVGFontFaceSrcElement",
            "SVGFontFaceUriElement",
            "SVGForeignObjectElement",
            "SVGGElement",
            "SVGGeometryElement",
            "SVGGlyphElement",
            "SVGGlyphRefElement",
            "SVGGradientElement",
            "SVGGraphicsElement",
            "SVGHKernElement",
            "SVGImageElement",
            "SVGLength",
            "SVGLengthList",
            "SVGLinearGradientElement",
            "SVGLineElement",
            "SVGMarkerElement",
            "SVGMaskElement",
            "SVGMetadataElement",
            "SVGMissingGlyphElement",
            "SVGMPathElement",
            "SVGNumber",
            "SVGNumberList",
            "SVGPathElement",
            "SVGPatternElement",
            "SVGPoint",
            "SVGPointList",
            "SVGPolygonElement",
            "SVGPolylineElement",
            "SVGPreserveAspectRatio",
            "SVGRadialGradientElement",
            "SVGRect",
            "SVGRectElement",
            "SVGRenderingIntent",
            "SVGScriptElement",
            "SVGSetElement",
            "SVGStopElement",
            "SVGStringList",
            "SVGStyleElement",
            "SVGSVGElement",
            "SVGSwitchElement",
            "SVGSymbolElement",
            "SVGTextContentElement",
            "SVGTextElement",
            "SVGTextPathElement",
            "SVGTextPositioningElement",
            "SVGTitleElement",
            "SVGTransform",
            "SVGTransformList",
            "SVGTRefElement",
            "SVGTSpanElement",
            "SVGUnitTypes",
            "SVGUseElement",
            "SVGViewElement",
            "SVGVKernElement",
            "SyncEvent",
            "SyncManager",
            "TaskAttributionTiming",
            "TaskController",
            "TaskPriorityChangeEvent",
            "TaskSignal",
            "Text",
            "TextDecoder",
            "TextDecoderStream",
            "TextEncoder",
            "TextEncoderStream",
            "TextFormat",
            "TextFormatUpdateEvent",
            "TextMetrics",
            "TextTrack",
            "TextTrackCue",
            "TextTrackCueList",
            "TextTrackList",
            "TextUpdateEvent",
            "TimeEvent",
            "TimeRanges",
            "ToggleEvent",
            "Touch",
            "TouchEvent",
            "TouchList",
            "TrackEvent",
            "TransformStream",
            "TransformStreamDefaultController",
            "TransitionEvent",
            "TreeWalker",
            "TrustedHTML",
            "TrustedScript",
            "TrustedScriptURL",
            "TrustedTypePolicy",
            "TrustedTypePolicyFactory",
            "UIEvent",
            "URL",
            "URLPattern",
            "URLSearchParams",
            "USB",
            "USBAlternateInterface",
            "USBConfiguration",
            "USBConnectionEvent",
            "USBDevice",
            "USBEndpoint",
            "USBInterface",
            "USBInTransferResult",
            "USBIsochronousInTransferPacket",
            "USBIsochronousInTransferResult",
            "USBIsochronousOutTransferPacket",
            "USBIsochronousOutTransferResult",
            "USBOutTransferResult",
            "UserActivation",
            "ValidityState",
            "VideoColorSpace",
            "VideoDecoder",
            "VideoEncoder",
            "VideoFrame",
            "VideoPlaybackQuality",
            "VideoTrack",
            "VideoTrackList",
            "ViewTimeline",
            "ViewTransition",
            "VirtualKeyboard",
            "VisibilityStateEntry",
            "VisualViewport",
            "VRDisplay",
            "VRDisplayCapabilities",
            "VRDisplayEvent",
            "VREyeParameters",
            "VRFieldOfView",
            "VRFrameData",
            "VRLayerInit",
            "VRPose",
            "VRStageParameters",
            "VTTCue",
            "VTTRegion",
            "WakeLock",
            "WakeLockSentinel",
            "WaveShaperNode",
            "WebGL2RenderingContext",
            "WebGLActiveInfo",
            "WebGLBuffer",
            "WebGLContextEvent",
            "WebGLFramebuffer",
            "WebGLObject",
            "WebGLProgram",
            "WebGLQuery",
            "WebGLRenderbuffer",
            "WebGLRenderingContext",
            "WebGLSampler",
            "WebGLShader",
            "WebGLShaderPrecisionFormat",
            "WebGLSync",
            "WebGLTexture",
            "WebGLTransformFeedback",
            "WebGLUniformLocation",
            "WebGLVertexArrayObject",
            "WebSocket",
            "WebTransport",
            "WebTransportBidirectionalStream",
            "WebTransportDatagramDuplexStream",
            "WebTransportError",
            "WebTransportReceiveStream",
            "WebTransportSendStream",
            "WGSLLanguageFeatures",
            "WheelEvent",
            "Window",
            "WindowClient",
            "WindowControlsOverlay",
            "WindowControlsOverlayGeometryChangeEvent",
            "WindowSharedStorage",
            "Worker",
            "WorkerGlobalScope",
            "WorkerLocation",
            "WorkerNavigator",
            "Worklet",
            "WorkletGlobalScope",
            "WorkletSharedStorage",
            "WritableStream",
            "WritableStreamDefaultController",
            "WritableStreamDefaultWriter",
            "XMLDocument",
            "XMLHttpRequest",
            "XMLHttpRequestEventTarget",
            "XMLHttpRequestUpload",
            "XMLSerializer",
            "XPathEvaluator",
            "XPathException",
            "XPathExpression",
            "XPathNSResolver",
            "XPathResult",
            "XRAnchor",
            "XRAnchorSet",
            "XRBoundedReferenceSpace",
            "XRCompositionLayer",
            "XRCPUDepthInformation",
            "XRCubeLayer",
            "XRCylinderLayer",
            "XRDepthInformation",
            "XREquirectLayer",
            "XRFrame",
            "XRHand",
            "XRHitTestResult",
            "XRHitTestSource",
            "XRInputSource",
            "XRInputSourceArray",
            "XRInputSourceEvent",
            "XRInputSourcesChangeEvent",
            "XRJointPose",
            "XRJointSpace",
            "XRLayer",
            "XRLayerEvent",
            "XRLightEstimate",
            "XRLightProbe",
            "XRMediaBinding",
            "XRPose",
            "XRProjectionLayer",
            "XRQuadLayer",
            "XRRay",
            "XRReferenceSpace",
            "XRReferenceSpaceEvent",
            "XRRenderState",
            "XRRigidTransform",
            "XRSession",
            "XRSessionEvent",
            "XRSpace",
            "XRSubImage",
            "XRSystem",
            "XRTransientInputHitTestResult",
            "XRTransientInputHitTestSource",
            "XRView",
            "XRViewerPose",
            "XRViewport",
            "XRWebGLBinding",
            "XRWebGLDepthInformation",
            "XRWebGLLayer",
            "XRWebGLSubImage",
            "XSLTProcessor"
        ];

        public static readonly string[] JavaScriptDeprecatedTypes =
        [
            "Plugin"
        ];

        public static readonly string[] JavaScriptUndocumentedMethods =
        [
            "Document.queryCommandIndeterm",
            "Document.queryCommandValue",
            "Document.captureEvents",
            "Document.releaseEvents",
            "Iterator.throw",
            "Iterator.next",
            "Iterator.return",
            "Element.webkitMatchesSelector"
        ];

        public static readonly string[] JavaScriptUndocumentedProperties =
        [
            "Node.DOCUMENT_POSITION_IMPLEMENTATION_SPECIFIC",
            "Node.DOCUMENT_POSITION_CONTAINS",
            "Node.DOCUMENT_TYPE_NODE",
            "Node.DOCUMENT_POSITION_FOLLOWING",
            "Node.DOCUMENT_POSITION_CONTAINED_BY",
            "Node.DOCUMENT_POSITION_PRECEDING",
            "Node.DOCUMENT_POSITION_DISCONNECTED",
            "Node.DOCUMENT_TYPE_NODE",
            "Range.END_TO_START",
            "Range.START_TO_START",
            "Range.START_TO_END",
            "Range.END_TO_END",
            "Node.ATTRIBUTE_NODE",
            "Node.DOCUMENT_FRAGMENT_NODE",
            "Node.PROCESSING_INSTRUCTION_NODE",
            "Node.NOTATION_NODE",
            "Node.ENTITY_NODE",
            "Node.ELEMENT_NODE",
            "Node.DOCUMENT_NODE",
            "Node.TEXT_NODE",
            "Node.CDATA_SECTION_NODE",
            "Node.COMMENT_NODE",
            "Node.ENTITY_REFERENCE_NODE",
            "HTMLTableCellElement.axis",
            "HTMLDivElement.align",
            "HTMLTableColElement.width",
            "HTMLTableCellElement.height",
            "HTMLTableCellElement.width",
            "Iterator.throw",
            "Iterator.next",
            "Iterator.return"
        ];

        public virtual string GetUrl(TypeScriptTypeDeclaration item)
        {
            string typeName = item.Name;

            if (typeName.Contains('<'))
            {
                typeName = typeName[..typeName.IndexOf('<')];
            }

            if (JavaScriptTypeMappings.TryGetValue(typeName, out string? value))
            {
                typeName = value;
            }

            string? url = GetMDNUrl(item);

            if (!String.IsNullOrEmpty(url))
            {
                return url;
            }

            if (typeName is "GlobalEventHandlers" or "GlobalEventHandlersEventMap" or "ElementEventMap")
            {
                return "https://developer.mozilla.org/docs/Web/HTML/Global_attributes";
            }

            else if (JavaScriptGlobalObjects.Contains(typeName))
            {
                return $"https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/{typeName.Replace(".", "/")}";
            }

            else if (JavaScriptBuiltinTypes.Contains(typeName))
            {
                return $"https://developer.mozilla.org/en-US/docs/Web/API/{typeName}";
            }

            else if (TypeScriptTypes.TryGetValue(typeName, out string? typeScriptTypeAnchor))
            {
                return $"https://www.typescriptlang.org/docs/handbook/utility-types.html#{typeScriptTypeAnchor}";
            }

            throw new Exception($"No MDN URL mapping for type {item.FullName}.");
        }

        public virtual string GetUrl(TypeScriptInterface item)
        {
            string typeName = item.Name;

            if (typeName.Contains('<'))
            {
                typeName = typeName[..typeName.IndexOf('<')];
            }

            string? url = GetMDNUrl(item);

            if (!String.IsNullOrEmpty(url))
            {
                return url;
            }

            if (typeName is "GlobalEventHandlers" or "GlobalEventHandlersEventMap" or "ElementEventMap")
            {
                return "https://developer.mozilla.org/docs/Web/HTML/Global_attributes";
            }

            if (JavaScriptGlobalObjects.Contains(typeName))
            {
                return $"https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/{typeName}";
            }

            else if (JavaScriptBuiltinTypes.Contains(typeName))
            {
                return $"https://developer.mozilla.org/en-US/docs/Web/API/{typeName}";
            }

            throw new Exception($"No MDN URL mapping for type {item.FullName}.");
        }

        public virtual string GetUrl(TypeScriptMethod item)
        {
            TypeScriptTypeDeclaration sourceType = ReferenceType.AllTypes[item.SourceTypeId];
            string typeName = sourceType.Name;

            if (typeName.Contains('<'))
            {
                typeName = typeName[..typeName.IndexOf('<')];
            }

            string? url = GetMDNUrl(item);

            if (!String.IsNullOrEmpty(url))
            {
                return url;
            }

            if (JavaScriptTypeMappings.TryGetValue(typeName, out string? value))
            {
                typeName = value;
            }

            if (typeName == "HTMLElement" && (item.Name == "removeEventListener" || item.Name == "addEventListener"))
            {
                return $"https://developer.mozilla.org/en-US/docs/Web/API/EventTarget/{item.Name}";
            }

            else if (JavaScriptUndocumentedMethods.Contains($"{typeName}.{item.Name}"))
            {
                return "";
            }

            else if (JavaScriptDeprecatedTypes.Contains(typeName))
            {
                return $"https://developer.mozilla.org/en-US/docs/Web/API/{typeName}";
            }

            else if (JavaScriptGlobalObjects.Contains(typeName))
            {
                return $"https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/{typeName}/{item.Name}";
            }

            else if (JavaScriptBuiltinTypes.Contains(typeName))
            {
                return $"https://developer.mozilla.org/en-US/docs/Web/API/{typeName}/{item.Name}";
            }

            throw new Exception($"No MDN URL mapping for method {sourceType.FullName}.{item.Name}().");
        }

        public virtual string GetUrl(TypeScriptProperty item)
        {
            TypeScriptTypeDeclaration sourceType = ReferenceType.AllTypes[item.SourceTypeId];
            string typeName = sourceType.Name;

            if (typeName.Contains('<'))
            {
                typeName = typeName[..typeName.IndexOf('<')];
            }

            string? url = GetMDNUrl(item);

            if (!String.IsNullOrEmpty(url))
            {
                return url;
            }

            if (typeName is "GlobalEventHandlers" or "GlobalEventHandlersEventMap" or "ElementEventMap")
            {
                return "https://developer.mozilla.org/docs/Web/HTML/Global_attributes";
            }

            if (JavaScriptTypeMappings.TryGetValue(typeName, out string? value))
            {
                typeName = value;
            }

            if (typeName == "HTMLElement" && item.Name.StartsWith("onwebkit"))
            {
                return "";
            }

            if (JavaScriptUndocumentedProperties.Contains($"{typeName}.{item.Name}"))
            {
                return "";
            }

            if (JavaScriptDeprecatedTypes.Contains(typeName))
            {
                return $"https://developer.mozilla.org/en-US/docs/Web/API/{typeName}";
            }

            else if (JavaScriptGlobalObjects.Contains(typeName))
            {
                return $"https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/{typeName}/{item.Name}";
            }

            else if (JavaScriptBuiltinTypes.Contains(typeName))
            {
                return $"https://developer.mozilla.org/en-US/docs/Web/API/{typeName}/{item.Name}";
            }

            throw new Exception($"No MDN URL mapping for property {sourceType.FullName}.{item.Name}.");
        }

        public virtual string GetUrl(TypeScriptMethodSignature item)
        {
            TypeScriptTypeDeclaration sourceType = ReferenceType.AllTypes[item.SourceTypeId];
            string typeName = sourceType.Name;

            if (typeName.Contains('<'))
            {
                typeName = typeName[..typeName.IndexOf('<')];
            }

            string? url = GetMDNUrl(item);

            if (!String.IsNullOrEmpty(url))
            {
                return url;
            }

            if (JavaScriptTypeMappings.TryGetValue(typeName, out string? value))
            {
                typeName = value;
            }

            if (typeName == "HTMLElement" && (item.Name == "removeEventListener" || item.Name == "addEventListener"))
            {
                return $"https://developer.mozilla.org/en-US/docs/Web/API/EventTarget/{item.Name}";
            }

            else if (JavaScriptUndocumentedMethods.Contains($"{typeName}.{item.Name}"))
            {
                return "";
            }

            else if (JavaScriptDeprecatedTypes.Contains(typeName))
            {
                return $"https://developer.mozilla.org/en-US/docs/Web/API/{typeName}";
            }

            else if (JavaScriptGlobalObjects.Contains(typeName))
            {
                return $"https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/{typeName}/{item.Name}";
            }

            else if (JavaScriptBuiltinTypes.Contains(typeName))
            {
                return $"https://developer.mozilla.org/en-US/docs/Web/API/{typeName}/{item.Name}";
            }

            throw new Exception($"No MDN URL mapping for method {sourceType.FullName}.{item.Name}().");
        }

        public virtual string GetUrl(IntrinsicType item)
        {
            return item.Name is "any" or "unknown" or "never"
                ? ""
                : item.Name == "void"
                    ? "https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Operators/void"
                    : $"https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/{item.Name}";
        }

        protected virtual string? GetMDNUrl(TypeScriptTypeDeclaration member)
        {
            if (member.Comment != null && member.Comment.SummaryMarkdown != null && member.Comment.SummaryMarkdown.Any(m => m.Text.Contains("[MDN Reference](")))
            {
                CommentSegment mdnLinkSegment = member.Comment.SummaryMarkdown.First(m => m.Text.Contains("[MDN Reference]("));
                int startIndex = mdnLinkSegment.Text.IndexOf("[MDN Reference](") + 16;
                int endIndex = mdnLinkSegment.Text.IndexOf(')', startIndex);
                string mdnUrl = mdnLinkSegment.Text[startIndex..endIndex];

                if (MDNUrlMappings.TryGetValue(mdnUrl, out string? value))
                {
                    mdnUrl = value;
                    mdnLinkSegment.Text = "[MDN Reference](" + mdnUrl + ")";
                }

                return mdnUrl;
            }

            return null;
        }

        protected virtual string? GetMDNUrl(TypeScriptCodeElement member)
        {
            if (member.Comment != null && member.Comment.SummaryMarkdown != null && member.Comment.SummaryMarkdown.Any(m => m.Text.Contains("[MDN Reference](")))
            {
                CommentSegment mdnLinkSegment = member.Comment.SummaryMarkdown.First(m => m.Text.Contains("[MDN Reference]("));
                int startIndex = mdnLinkSegment.Text.IndexOf("[MDN Reference](") + 16;
                int endIndex = mdnLinkSegment.Text.IndexOf(')', startIndex);
                string mdnUrl = mdnLinkSegment.Text[startIndex..endIndex];

                if (MDNUrlMappings.TryGetValue(mdnUrl, out string? value))
                {
                    mdnUrl = value;
                    mdnLinkSegment.Text = "[MDN Reference](" + mdnUrl + ")";
                }

                return mdnUrl;
            }

            return null;
        }
    }
}
