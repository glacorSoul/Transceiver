// This file is part of Transceiver.
// Transceiver is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// Transceiver is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY

using System.Security.Cryptography.X509Certificates;

namespace Transceiver;

public class CertificateLoader : ICertificateLoader
{
    private readonly StoreName _store;
    private readonly StoreLocation _location;
    private readonly X509FindType _strategy;
    public CertificateLoader(StoreName store, StoreLocation location, X509FindType strategy)
    {
        _store = store;
        _location = location;
        _strategy = strategy;
    }

    public X509Certificate2 LoadCertificate(string value)
    {
        X509Store certStore = new(_store, _location);
        certStore.Open(OpenFlags.ReadOnly);
        X509Certificate2Collection certificates = certStore.Certificates.Find(_strategy, value, true);
        if (certificates.Count == 0)
        {
            throw new FileNotFoundException("Certificate not found");
        }
        return certificates[0];
    }
}
