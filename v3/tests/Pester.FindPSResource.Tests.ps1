# This is a Pester test suite to validate Find-PSResource.
#
# Copyright (c) Microsoft Corporation, 2020

#Import-Module "$PSScriptRoot\PSGetTestUtils.psm1" -WarningAction SilentlyContinue

$PSGalleryName = 'PSGallery'
$PSGalleryLocation = 'https://www.powershellgallery.com/api/v2'

$TestRepoNameV2 = 'TestRepoName'
$TestRepoURLV2 = 'https://www.poshtestgallery.com/api/v2'

$TestRepoNameV3 = "NuGet"
$TestRepoURLV3 = 'https://api.nuget.org/v3/index.json'

$TestRepoLocalName = 'TestLocalRepo'
$tmpdir = Join-Path -Path ([System.IO.Path]::GetTempPath()) -ChildPath $TestRepoLocalName
if (-not (Test-Path -LiteralPath $tmpdir)) {
    New-Item -Path $tmpdir -ItemType Directory > $null
    $TestRepoLocalURL = $tmpdir
}

$TestRepoLocalName2 = "TestLocalRepoName2"
$tmpdir2 = Join-Path -Path ([System.IO.Path]::GetTempPath()) -ChildPath $TestRepoLocalName2
if (-not (Test-Path -LiteralPath $tmpdir2)) {
    New-Item -Path $tmpdir2 -ItemType Directory > $null
    $TestRepoLocalURL2 = $tmpdir2
}

# remember to delete these files
#    Remove-Item -LiteralPath $tmpdir -Force -Recurse
#}




####################################################
### Find-PSResource using the PowerShell Gallery ###
####################################################

Describe 'Test Find-PSResource interaction with the PowerShell Gallery' -tags 'BVT' {

    BeforeAll {
        Register-PSResourceRepository -PSGallery
    }
    AfterAll {

    }

    BeforeEach {

    }

    It 'Should find a PSResource' {
        $pkg = Find-PSResource -Repository PSGallery -Name PackageManagement

        $pkg.Count | should -Be 1
        $pkg.Name | should -Be "PackageManagement"
    }

    It 'Should find all PSResources given a tag' {
        $pkgs = Find-PSResource -Repository PSGallery -Tags "Azure"

        $pkgs.Count | should -BeGreaterThan 0
        $pkgs.Name | should -Contain "Metis"
    }

    It 'Should find a PSResource given a name and a tag' {
        $pkg = Find-PSResource -Repository PSGallery -Name Windows_PowerShell_Profile_Loader_Installer  -Tags 'RMS'

        $pkg.Count | should -Be 1
        $pkg.Name | should -Be "Windows_PowerShell_Profile_Loader_Installer"
    }

    It 'Should fail to find a PSResource that does not have the tag specified' {
        $pkg = Find-PSResource -Repository PSGallery -Name Windows_PowerShell_Profile_Loader_Installer -Tags "Azure"

        $pkg | should -BeNullOrEmpty
    }

    It 'Should only find PSResource with tag when specifiying multiple names' {
        $pkg = Find-PSResource -Repository PSGallery -Name WFTools, Windows_PowerShell_Profile_Loader_Installer -Tags "Azure"

        $pkg.Count | should -Be 1
        $pkg.Name | should -Be "WFTools"
    }

    It 'Should find a module when specifying module name' {
        $pkg = Find-PSResource -Repository PSGallery -ModuleName PackageManagement

        $pkg.Count | should -Be 1
        $pkg.Name | should -Be "PackageManagement"
    }

    It 'Should find a module when specifying module name and type' {
        $pkg = Find-PSResource -Repository PSGallery -ModuleName PackageManagement -Type Module

        $pkg.Count | should -Be 1
        $pkg.Name | should -Be "PackageManagement"
    }

    ##### TODO: This should return the dsc resource ???
    It 'Should find a module when specifying module name and a DSCResource name' {
        $pkg = Find-PSResource -Repository PSGallery -name MSFT_PackageManagementSource -ModuleName PackageManagement

        $pkg.Count | should -Be 1
        $pkg.Name | should -Be "PackageManagement"
    }

    ##### TODO: This should return the dsc resource  ????
    It 'Should find a module when specifying module name, a DSCResource name, and a type of DSCResource' {
        $pkg = Find-PSResource -Repository PSGallery -name MSFT_PackageManagementSource -ModuleName PackageManagement -Type DscResource

        $pkg.Count | should -Be 1
        $pkg.Name | should -Be "PackageManagement"
    }

    It 'Should find a PSResource with the name specified that is within a version range (exclusive)' {
        $pkg = Find-PSResource -Repository PSGallery -Name WFTools -Version "(0.1.39, 0.1.56)"

        $pkg.Count | should -Be 1
        $pkg.Name | should -Be "WFTools"
        $pkg.Version | Should -BeGreaterThan "0.1.39"
        $pkg.Version | Should -BeLessThan "0.1.56"
    }

    It 'Should find a PSResource with the name specified that is within a version range (inclusive)' {
        $pkg = Find-PSResource -Repository PSGallery -Name WFTools -Version "[0.1.39, 0.1.56]"

        $pkg.Count | should -Be 1
        $pkg.Name | should -Be "WFTools"
        $pkg.Version | Should -BeGreaterOrEqual "0.1.39"
        $pkg.Version | Should -BeLessOrEqual "0.1.56"
    }

    It 'Should find all versions of a PSResource with the name specified' {
        $pkgs = Find-PSResource -Repository PSGallery -Name WFTools -Version *

        $pkgs.Count | should -BeGreaterThan 1
        $pkgs.Name | should -Be "WFTools"
    }

    It 'Should find the exact version of a PSResource with the name specified' {
        $pkg = Find-PSResource -Repository PSGallery -Name WFTools -Version 0.1.58

        $pkg.Count | should -Be 1
        $pkg.Name | should -Be "WFTools"
        $pkg.Version | should -Be "0.1.58"
    }

    It 'Should find the exact version of a PSResource with the name specified, using NuGet versioning syntax' {
        $pkg = Find-PSResource -Repository PSGallery -Name WFTools -Version "[0.1.1]"

        $pkg.Count | should -Be 1
        $pkg.Name | should -Be "WFTools"
        $pkg.Version | should -Be "0.1.1"
    }

    It 'Should find a PSResource with the name specified that is within a version range (inclusive)' {
        $pkg = Find-PSResource -Repository PSGallery -Name WFTools -Version "[0.1.1,9)"

        $pkg.Count | should -Be 1
        $pkg.Name | should -Be "WFTools"
        $pkg.Version | Should -BeGreaterOrEqual "0.1.39"
        $pkg.Version | Should -BeLessOrEqual "0.1.56"
    }





}
