namespace Greenseer.Exceptions;

public sealed class DocumentNotFoundException : Exception
{
  public DocumentNotFoundException(Type documentType, string uniqueKey) : base($"Could not find document of type {documentType.FullName} with unique key {uniqueKey}.")
  {
  }
}