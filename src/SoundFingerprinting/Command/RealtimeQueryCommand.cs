namespace SoundFingerprinting.Command
{
    using System.Collections.Concurrent;
    using System.Threading;
    using System.Threading.Tasks;
    using SoundFingerprinting.Audio;
    using SoundFingerprinting.Builder;
    using SoundFingerprinting.Configuration;

    public interface IRealtimeQueryCommand
    {
        Task Query(CancellationToken cancellationToken);
    }
    
    public class RealtimeQueryCommand : IRealtimeSource, IWithRealtimeQueryConfiguration, IUsingRealtimeQueryServices, IRealtimeQueryCommand
    {
        private readonly IQueryFingerprintService queryFingerprintService = QueryFingerprintService.Instance;
        private readonly IFingerprintCommandBuilder fingerprintCommandBuilder = FingerprintCommandBuilder.Instance;
        
        private BlockingCollection<AudioSamples> realtimeSamples;
        private RealtimeQueryConfiguration realtimeQueryConfiguration;
        private QueryConfiguration queryConfiguration;
        private IModelService modelService;
        private IAudioService audioService;
        
        public IWithRealtimeQueryConfiguration From(BlockingCollection<AudioSamples> audioSamples)
        {
            this.realtimeSamples = audioSamples;
            return this;
        }

        public IUsingRealtimeQueryServices WithRealtimeQueryConfig(RealtimeQueryConfiguration realtimeQueryConfiguration)
        {
            this.realtimeQueryConfiguration = realtimeQueryConfiguration;
            this.queryConfiguration = new DefaultQueryConfiguration
            {
                ThresholdVotes = realtimeQueryConfiguration.ThresholdVotes
            };
            return this;
        }

        public async Task Query(CancellationToken cancellationToken)
        {
            while (!realtimeSamples.IsAddingCompleted && !cancellationToken.IsCancellationRequested)
            {
                if (realtimeSamples.TryTake(out var audioSamples, realtimeQueryConfiguration.ApproximateChunkLength.Milliseconds, cancellationToken))
                {
                    var hashes = await fingerprintCommandBuilder.BuildFingerprintCommand()
                        .From(audioSamples)
                        .UsingServices(audioService)
                        .Hash();

                    var results = queryFingerprintService.Query(hashes, queryConfiguration, modelService);
                    
                    // TODO aggregate results over time
                    // Fire on passing callback
                }
            }
        }

        public IRealtimeQueryCommand UsingServices(IModelService modelService)
        {
            this.modelService = modelService;
            this.audioService = new SoundFingerprintingAudioService();
            return this;
        }
    }
}