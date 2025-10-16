namespace SewerDeformationSoftware.Logics.Converters;

class Text2Unbind : IValueConverter
{
    public Object? Convert(Object Value, Type TargetType, Object Parameter, CultureInfo Culture)
    {
        String? Path = Value as String;

        if (String.IsNullOrEmpty(Path))
        {
            return Binding.DoNothing;
        }

        return Path;
    }

    public Object ConvertBack(Object value, Type TargetType, Object Parameter, CultureInfo Culture)

                                                                              => Binding.DoNothing;
}