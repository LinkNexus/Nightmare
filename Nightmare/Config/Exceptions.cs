using Nightmare.Parser;

namespace Nightmare.Config;

public class ConfigNotFoundException(string fileName) : FileNotFoundException
    ($"The config file {fileName} could not be found in the current directory or any parent directory in the given range. Try calling the `new` command first.")
{
}

public class ConfigProcessingException(string message, TextSpan span) :
    TracedException(
        $"Error while processing the config: {message}",
        span
    )
{
}