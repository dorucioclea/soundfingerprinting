namespace SoundFingerprinting.Audio
{
    /// <summary>
    /// Utility interface which provides helper methods related to wave (.wav) file management
    /// </summary>
    public interface IWaveFileUtility
    {
        /// <summary>
        /// Write audio samples to wave (.wav) file in 32 bit format
        /// </summary>
        /// <param name="samples">Samples to write</param>
        /// <param name="sampleRate">Sample rate at which provided samples where gathered</param>
        /// <param name="destination">Path to destination file</param>
        void WriteSamplesToFile(float[] samples, int sampleRate, string destination);

        /// <summary>
        /// Write audio samples to wave (.wav) file in 16 bit format
        /// </summary>
        /// <param name="samples">Samples to write</param>
        /// <param name="sampleRate">Sample rate</param>
        /// <param name="destination">Path to destination file</param>
        void WriteSamplesToFile(short[] samples, int sampleRate, string destination);
    }
}