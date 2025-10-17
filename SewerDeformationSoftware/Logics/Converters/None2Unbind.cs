namespace SewerDeformationSoftware.Logics.Converters;

class None2Unbind : IValueConverter
{
    public Object Convert(Object Value, Type TargetType, Object Parameter, CultureInfo Culture)
    {
        if (Value is String ImagePath)
        {
            if (!String.IsNullOrWhiteSpace(ImagePath))
            {
                return ImagePath;
            }

            return Binding.DoNothing;
        }

        else

        if (Value is BitmapSource BS)
        {
            if (BS != null)
            {
                return BS;
            }

            return Binding.DoNothing;
        }

        else

        {
            return Binding.DoNothing;
        }
    }

    public Object ConvertBack(Object value, Type TargetType, Object Parameter, CultureInfo Culture)

                                                                              => Binding.DoNothing;
}