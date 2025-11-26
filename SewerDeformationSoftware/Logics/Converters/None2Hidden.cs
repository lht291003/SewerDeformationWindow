namespace SewerDeformationSoftware.Logics.Converters;

class None2Hidden : IValueConverter
{
    public Object Convert(Object Value, Type TargetType, Object Parameter, CultureInfo Culture)
    {
        if (Value == null)
        {
            return Visibility.Collapsed;
        }

        if (Value is String Path && String.IsNullOrWhiteSpace(Path))
        {
            return Visibility.Collapsed;
        }

        return Visibility.Visible;
    }

    public Object ConvertBack(Object value, Type TargetType, Object Parameter, CultureInfo Culture)

                                                                              => Binding.DoNothing;
}