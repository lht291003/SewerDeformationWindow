namespace SewerDeformationSoftware.Logics.Converters;

class Bool2Vision : IValueConverter
{
    public Object Convert(Object Value, Type TargetType, Object Parameter, CultureInfo Culture)
    {
        if (Parameter.ToString()!.Equals("Began"))
        {
            return Value is Boolean Trigger && Trigger ? Visibility.Collapsed : Visibility.Visible;
        }

        if (Parameter.ToString()!.Equals("Break"))
        {
            return Value is Boolean Trigger && Trigger ? Visibility.Visible : Visibility.Collapsed;
        }

        if (Parameter.ToString()!.Equals("Final"))
        {
            return Value is Boolean Trigger && Trigger ? Visibility.Visible : Visibility.Collapsed;
        }

        return Binding.DoNothing;
    }

    public Object ConvertBack(Object value, Type TargetType, Object Parameter, CultureInfo Culture)

                                                                              => Binding.DoNothing;
}