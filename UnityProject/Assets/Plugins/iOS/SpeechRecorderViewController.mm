//
//  SpeechRecorderViewController.m
//  SpeechToText
//
#import "SpeechRecorderViewController.h"
#import <Speech/Speech.h>
#import <mach/mach_time.h>
@interface SpeechRecorderViewController ()
{    
    // Speech recognize
    SFSpeechRecognizer *speechRecognizer;
    SFSpeechAudioBufferRecognitionRequest *recognitionRequest;
    SFSpeechRecognitionTask *recognitionTask;
    // Record speech using audio Engine
    AVAudioInputNode *inputNode;
    AVAudioEngine *audioEngine;	
	NSString * LanguageCode;
    int silenceLengthInMs;
    int minimumLengthInMs;
    int maximumLengthInMs;
    uint64_t lastTimeReturnResult;
}
@end

@implementation SpeechRecorderViewController
- (uint64_t) getCurrentTimeInMilliseconds {
    uint64_t now = mach_absolute_time();
    mach_timebase_info_data_t info;
    mach_timebase_info(&info);
    
    // Convert ticks to nanoseconds, then to milliseconds
    uint64_t timeInNanoseconds = now * info.numer / info.denom;
    uint64_t timeInMilliseconds = timeInNanoseconds / 1e6;

    return timeInMilliseconds;
}

- (id)init
{
	audioEngine = [[AVAudioEngine alloc] init];
    LanguageCode = @"ko-KR";
    NSLocale *local =[[NSLocale alloc] initWithLocaleIdentifier:LanguageCode];
    speechRecognizer = [[SFSpeechRecognizer alloc] initWithLocale:local];
    silenceLengthInMs = 0;
    minimumLengthInMs = 0;
    maximumLengthInMs = 0;
    //for (NSLocale *locate in [SFSpeechRecognizer supportedLocales]) {
    //    NSLog(@"%@", [locate localizedStringForCountryCode:locate.countryCode]);
    //}
	
    // Check Authorization Status
    // Make sure you add "Privacy - Microphone Usage Description" key and reason in Info.plist to request micro permison
    // And "NSSpeechRecognitionUsageDescription" key for requesting Speech recognize permison
    [SFSpeechRecognizer requestAuthorization:^(SFSpeechRecognizerAuthorizationStatus status) {
        //The callback may not be called on the main thread. Add an operation to the main queue to update the record button's state.
        dispatch_async(dispatch_get_main_queue(), ^{
            switch (status) {
                case SFSpeechRecognizerAuthorizationStatusAuthorized: {
                    NSLog(@"SUCCESS");
                    break;
                }
                case SFSpeechRecognizerAuthorizationStatusDenied: {
					NSLog(@"User denied access to speech recognition");
                    break;
                }
                case SFSpeechRecognizerAuthorizationStatusRestricted: {
					NSLog(@"User denied access to speech recognition");
                    break;
                }
                case SFSpeechRecognizerAuthorizationStatusNotDetermined: {
					NSLog(@"User denied access to speech recognition");
                    break;
                }
            }
        });
        
    }];

	return self;
}

- (void)SettingSpeech: (const char *) _language 
{	
    LanguageCode = [NSString stringWithUTF8String:_language];
    NSLocale *local =[[NSLocale alloc] initWithLocaleIdentifier:LanguageCode];
    speechRecognizer = [[SFSpeechRecognizer alloc] initWithLocale:local];
    NSLog(@"Setting language success");
    UnitySendMessage("SpeechToText", "onMessage", "Setting Success");
}
- (void)SettingSpeechSilenceLength: (int ) _silenceInMs
{
    silenceLengthInMs = _silenceInMs;
    NSLog(@"silenceLengthInMs: %d", silenceLengthInMs);
}
- (void)SettingSpeechMinimumLength: (int ) _minimumInMs
{
    minimumLengthInMs = _minimumInMs;
    NSLog(@"minimumLengthInMs: %d", minimumLengthInMs);
}
- (void)SettingSpeechMaximumLength: (int ) _maximumInMs
{
    maximumLengthInMs = _maximumInMs;
    NSLog(@"maximumLengthInMs: %d", maximumLengthInMs);
}

// recording
- (void)startRecording {
    if (!audioEngine.isRunning) {
        [inputNode removeTapOnBus:0];
        if (recognitionTask) {
            [recognitionTask cancel];
            recognitionTask = nil;
        }
        		
        AVAudioSession *session = [AVAudioSession sharedInstance];
        [session setCategory:AVAudioSessionCategoryPlayAndRecord withOptions:AVAudioSessionCategoryOptionDefaultToSpeaker|AVAudioSessionCategoryOptionMixWithOthers error:nil];
        [session setActive:TRUE error:nil];
        
        inputNode = audioEngine.inputNode;
        
        recognitionRequest = [[SFSpeechAudioBufferRecognitionRequest alloc] init];
        recognitionRequest.shouldReportPartialResults = YES;
        recognitionTask =[speechRecognizer recognitionTaskWithRequest:recognitionRequest resultHandler:^(SFSpeechRecognitionResult * _Nullable result, NSError * _Nullable error)
        {
            if (result) {
                lastTimeReturnResult = [self getCurrentTimeInMilliseconds];
                NSString *transcriptText = result.bestTranscription.formattedString;
                NSLog(@"STARTRECORDING PARTIAL RESULT: %@", transcriptText);
                if (result.isFinal) {
                    NSLog(@"STARTRECORDING FINAL RESULT: %@", transcriptText);
                    UnitySendMessage("SpeechToText", "onResults", [transcriptText UTF8String]);
                }
            }
            else {
                [audioEngine stop];
                recognitionTask = nil;
                recognitionRequest = nil;
                UnitySendMessage("SpeechToText", "onResults", "nil");
                NSLog(@"STARTRECORDING RESULT NULL");
            }
        }];
        __block int accumulatedDuration = 0;
        lastTimeReturnResult = 0;
        AVAudioFormat *format = [inputNode outputFormatForBus:0];
        
        [inputNode installTapOnBus:0 bufferSize:1024 format:format block:^(AVAudioPCMBuffer * _Nonnull buffer, AVAudioTime * _Nonnull when) {
            // Calculate buffer duration
            AVAudioFrameCount frameCount = buffer.frameLength;
            double sampleRate = format.sampleRate;
            int bufferDuration = (frameCount / sampleRate) * 1000;
        
            // Accumulate duration
            accumulatedDuration += bufferDuration;
            NSLog(@"accumulatedDuration: %d", accumulatedDuration);
            if (maximumLengthInMs > 0)
            {
                if (accumulatedDuration <= maximumLengthInMs) {
                    // Append audio buffer for recognition
                    NSLog(@"recognitionRequest appendAudioPCMBuffer:buffer");
                    [recognitionRequest appendAudioPCMBuffer:buffer];
                } else {
                    NSLog(@"Maximum length reached, stopRecording");
                    [self stopRecording];
                }
            }
            else {
                NSLog(@"recognitionRequest appendAudioPCMBuffer:buffer");
                [recognitionRequest appendAudioPCMBuffer:buffer];
            }
            // Reset timeout timer when audio buffer is received (indicating activity)
            if (silenceLengthInMs > 0 && lastTimeReturnResult > 0) 
            {
                if ([self getCurrentTimeInMilliseconds] - lastTimeReturnResult >= silenceLengthInMs && accumulatedDuration >= minimumLengthInMs) {
                    NSLog(@"Silence timeout reached, stop recording");
                    [self stopRecording];
                }
            }
        }];
        [audioEngine prepare];
        NSError *error1;
        [audioEngine startAndReturnError:&error1];

        if (error1.description) {
            NSLog(@"errorAudioEngine.description: %@", error1.description);
        }
    }
}

- (void)stopRecording {
    if (audioEngine.isRunning) {
        [inputNode removeTapOnBus:0];
		[audioEngine stop];
        [recognitionRequest endAudio];
    }
}

@end
extern "C"{
    SpeechRecorderViewController *vc = nil;
    
    SpeechRecorderViewController *getVc() {
        if (vc == nil) {
            vc = [[SpeechRecorderViewController alloc] init];
        }
        
        return vc;
    }
    
    void _TAG_startRecording(){
        SpeechRecorderViewController *pVc = getVc();
        [pVc startRecording];
    }
    void _TAG_stopRecording(){
        SpeechRecorderViewController *pVc = getVc();
        [pVc stopRecording];
    }
    void _TAG_SettingSpeech(const char * _language){
        SpeechRecorderViewController *pVc = getVc();
        [pVc SettingSpeech:_language];
    }
    void _TAG_SettingSpeechSilenceLength(int _silenceInMs){
        SpeechRecorderViewController *pVc = getVc();
        [pVc SettingSpeechSilenceLength:_silenceInMs];
    }
    void _TAG_SettingSpeechMinimumLength(int _minimumInMs){
        SpeechRecorderViewController *pVc = getVc();
        [pVc SettingSpeechMinimumLength:_minimumInMs];
    }
    void _TAG_SettingSpeechMaximumLength(int _maximumInMs){
        SpeechRecorderViewController *pVc = getVc();
        [pVc SettingSpeechMaximumLength:_maximumInMs];
    }
}
