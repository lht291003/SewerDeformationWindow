namespace SewerDeformationSoftware.Logics.Converters
{
    public class Text2Vision : IValueConverter
    {
        public Object Convert(Object Value, Type TargetType, Object Parameter, CultureInfo Culture)
        {
            if (Value is String Text && !String.IsNullOrWhiteSpace(Text))
            {
                return Visibility.Visible;
            }
            else
            {
                return Visibility.Hidden;
            }
        }

        public Object ConvertBack(Object value, Type TargetType, Object Parameter, CultureInfo Culture)

                                                                                  => Binding.DoNothing;
    }
}