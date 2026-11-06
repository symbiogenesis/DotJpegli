%global debug_package %{nil}
%define __strip /bin/true
%define __os_install_post %{nil}
%define _build_id_links none

Name:           dotjpegli
Version:        %{pkg_version}
Release:        1
Summary:        Native JPEG encoder based on Google Jpegli
License:        BSD-3-Clause
URL:            https://github.com/symbiogenesis/DotJpegli
AutoReqProv:    no

%description
Self-contained NativeAOT command-line JPEG encoder built from DotJpegli.
No .NET runtime installation is required.

%install
mkdir -p "%{buildroot}/usr/bin"
mkdir -p "%{buildroot}/usr/share/doc/dotjpegli"
install -m 0755 "%{payload_dir}/dotjpegli" "%{buildroot}/usr/bin/dotjpegli"
install -m 0644 "%{payload_dir}/LICENSE" "%{buildroot}/usr/share/doc/dotjpegli/LICENSE"
install -m 0644 "%{payload_dir}/LICENSE.jpegli" "%{buildroot}/usr/share/doc/dotjpegli/LICENSE.jpegli"
install -m 0644 "%{payload_dir}/THIRD-PARTY-NOTICES.md" "%{buildroot}/usr/share/doc/dotjpegli/THIRD-PARTY-NOTICES.md"
install -m 0644 "%{payload_dir}/LICENSE.dotnet.txt" "%{buildroot}/usr/share/doc/dotjpegli/LICENSE.dotnet.txt"
install -m 0644 "%{payload_dir}/THIRD-PARTY-NOTICES.dotnet.txt" "%{buildroot}/usr/share/doc/dotjpegli/THIRD-PARTY-NOTICES.dotnet.txt"
install -m 0644 "%{payload_dir}/README.md" "%{buildroot}/usr/share/doc/dotjpegli/README.md"

%files
%defattr(-,root,root,-)
%attr(0755,root,root) /usr/bin/dotjpegli
%dir /usr/share/doc/dotjpegli
%attr(0644,root,root) /usr/share/doc/dotjpegli/LICENSE
%attr(0644,root,root) /usr/share/doc/dotjpegli/LICENSE.jpegli
%attr(0644,root,root) /usr/share/doc/dotjpegli/THIRD-PARTY-NOTICES.md
%attr(0644,root,root) /usr/share/doc/dotjpegli/LICENSE.dotnet.txt
%attr(0644,root,root) /usr/share/doc/dotjpegli/THIRD-PARTY-NOTICES.dotnet.txt
%attr(0644,root,root) /usr/share/doc/dotjpegli/README.md
