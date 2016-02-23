//
// PSTException
//
// Defines a new type of exception to be used throughout the program.
//


// [Serializable()]
public class PSTException : System.Exception
{
    public PSTException() : base() { }
    public PSTException(string message) : base(message) { }
    public PSTException(string message, System.Exception inner) : base(message, inner) { }

    // A constructor is needed for serialization when an 
    // exception propagates from a remoting server to the client.  
//    protected KalahaException(System.Runtime.Serialization.SerializationInfo info,
//    System.Runtime.Serialization.StreamingContext context) { }
}
