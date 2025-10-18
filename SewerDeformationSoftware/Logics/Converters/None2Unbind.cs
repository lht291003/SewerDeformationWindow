namespace SewerDeformationSoftware.Logics.Converters;

class None2Unbind : IValueConverter
{
    public Object Convert(Object Value, Type TargetType, Object Parameter, CultureInfo Culture)
    {
        if (Value == null)
        {
            return Binding.DoNothing;
        }

        if (Value is String Path && String.IsNullOrWhiteSpace(Path))
        {
            return Binding.DoNothing;
        }

        return Value;
    }

    public Object ConvertBack(Object value, Type TargetType, Object Parameter, CultureInfo Culture)

                                                                              => Binding.DoNothing;
}