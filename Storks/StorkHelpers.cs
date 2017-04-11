using Storks.Encoders;

namespace Storks
{
    /// <summary>
    /// A collection of methods for useful functions when working with Stork
    /// </summary>
    public static class StorkHelpers
    {
        /// <summary>
        /// Registers encoders for many standard types
        /// </summary>
        /// <param name="controller">The controller to register the encoders to</param>
        /// <exception cref="System.ArgumentNullException">if <paramref name="controller"/> is null</exception>
        public static void RegisterDefaultEncoders(this IStoreBackedPropertyController controller)
        {
            Throw.IfNull(() => controller);
            controller.RegisterEncoder(new StringStoreBackedPropertyEncoder());
        }
    }
}