namespace SewerDeformationSoftware.Logics.Converters;

class Radios2Text : IValueConverter
{
    public Object Convert(Object Value, Type TargetType, Object Parameter, CultureInfo Culture)

                                  => Value is String Text && Text.Equals(Parameter.ToString());

    public Object ConvertBack(Object Value, Type TargetType, Object Parameter, CultureInfo Culture)

        => Value is Boolean Checked && Checked == true ? Parameter.ToString()! : Binding.DoNothing;
}