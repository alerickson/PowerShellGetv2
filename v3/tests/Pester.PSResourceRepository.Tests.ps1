# This is a Pester test suite to validate Register-PSResourceRepository, Unregister-PSResourceRepository, Get-PSResourceRepository, and Set-PSResourceRepository.
#
# Copyright (c) Microsoft Corporation, 2019

#Import-Module "$PSScriptRoot\PSGetTestUtils.psm1" -WarningAction SilentlyContinue

$PSGalleryName = 'PSGallery'
$PSGalleryLocation = 'https://www.powershellgallery.com/api/v2'

$TestRepoName = 'TestRepoName'
$TestRepoURL = 'https://api.poshtestgallery.com/v3/index.json'

$TestRepoName2 = "NuGet"
$TestRepoURL2 = 'https://api.nuget.org/v3/index.json'

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




#####################################
### Register-PSResourceRepository ###
#####################################

Describe 'Test Register-PSResourceRepository' -tags 'BVT' {

    BeforeAll {

    }
    AfterAll {
        Unregister-PSResourceRepository -Name $PSGalleryName -ErrorAction SilentlyContinue
        Unregister-PSResourceRepository -Name $TestRepoName -ErrorAction SilentlyContinue
        Unregister-PSResourceRepository -Name $TestRepoName2 -ErrorAction SilentlyContinue
        Unregister-PSResourceRepository -Name $TestRepoLocalName -ErrorAction SilentlyContinue
        Unregister-PSResourceRepository -Name $TestRepoLocalName2 -ErrorAction SilentlyContinue
    }

    BeforeEach {
        Unregister-PSResourceRepository -Name $PSGalleryName -ErrorAction SilentlyContinue
        Unregister-PSResourceRepository -Name $TestRepoName -ErrorAction SilentlyContinue
        Unregister-PSResourceRepository -Name $TestRepoName2 -ErrorAction SilentlyContinue
        Unregister-PSResourceRepository -Name $TestRepoLocalName -ErrorAction SilentlyContinue
        Unregister-PSResourceRepository -Name $TestRepoLocalName2 -ErrorAction SilentlyContinue
    }

    ### Registering the PowerShell Gallery
    It 'Should register the default PSGallery' {
        Register-PSResourceRepository -PSGallery

        $repo = Get-PSResourceRepository $PSGalleryName
        $repo | Should -Not -BeNullOrEmpty
        $repo.URL | Should be $PSGalleryLocation
        $repo.Trusted | Should be $false
        $repo.Priority | Should be 50
    }

    It 'Should register PSGallery with installation policy trusted' {
        Register-PSResourceRepository -PSGallery -Trusted

        $repo = Get-PSResourceRepository $PSGalleryName
        $repo.Name | Should be $PSGalleryName
        $repo.Trusted | Should be $false
    }

    It 'Should fail to reregister PSGallery' {
        Register-PSResourceRepository -PSGallery
        Register-PSResourceRepository -PSGallery -ErrorVariable ev -ErrorAction SilentlyContinue

        $ev[0].FullyQualifiedErrorId | Should be "The PSResource Repository 'PSGallery' already exists."
    }

    It 'Should fail to register PSGallery when manually providing URL' {
        Register-PSResourceRepository $PSGalleryName -URL $PSGalleryLocation -ErrorVariable ev -ErrorAction SilentlyContinue

        $ev[0].FullyQualifiedErrorId | Should be "Use 'Register-PSResourceRepository -Default' to register the PSGallery repository."
    }


    ### Registering an online URL
    It 'Should register the test repository with online -URL' {
        Register-PSResourceRepository $TestRepoName -URL $TestRepoURL

        $repo = Get-PSResourceRepository $TestRepoName
        $repo.Name | should be $TestRepoName
        $repo.URL | should be $TestRepoURL
        $repo.Trusted | should be $false
    }

    It 'Should register the test repository when -URL is a website and installation policy is trusted' {
        Register-PSResourceRepository $TestRepoName -URL $TestRepoURL -Trusted

        $repo = Get-PSResourceRepository $TestRepoName
        $repo.Name | should be $TestRepoName
        $repo.URL | should be $TestRepoURL
        $repo.Trusted | should be $true
    }

    It 'Should register the test repository when -URL is a website and priority is set' {
        Register-PSResourceRepository $TestRepoName -URL $TestRepoURL -Priority 2

        $repo = Get-PSResourceRepository $TestRepoName
        $repo.Name | should be $TestRepoName
        $repo.URL | should be $TestRepoURL
        $repo.Trusted | should be $true
        $repo.Priority | should be 2
    }

    It 'Should fail to reregister the repository when the -Name is already registered' {
        Register-PSResourceRepository $TestRepoName -URL $TestRepoURL
        Register-PSResourceRepository $TestRepoName -URL $TestRepoURL2 -ErrorVariable ev -ErrorAction SilentlyContinue

        $ev[0].FullyQualifiedErrorId | Should be "The PSResource Repository '$($TestRepoName)' exists."
    }

    It 'Should fail to reregister the repository when the -URL is already registered' {
        Register-PSResourceRepository $TestRepoName -URL $TestRepoURL
        Register-PSResourceRepository $TestRepoName2 -URL $TestRepoURL -ErrorVariable ev -ErrorAction SilentlyContinue

        $ev[0].FullyQualifiedErrorId | Should be "The repository could not be registered because there exists a registered repository with Name '$($TestRepoName)' and URL '$($TestRepoURL)'. To register another repository with Name '$($TestRepoName2)', please unregister the existing repository using the Unregister-PSResourceRepository cmdlet."
    }


    ### Registering a fileshare URL
    It 'Should register the test repository when -URL is a fileshare' {
        Register-PSResourceRepository $TestRepoLocalName -URL $TestRepoLocalURL

        $repo = Get-PSResourceRepository $TestRepoLocalName
        $repo.Name | should be $TestRepoLocalName
        $repo.URL | should be $TestRepoLocalURL
        $repo.Trusted | should be $false
    }

    It 'Should register the test repository when -URL is a fileshare and installation policy is trusted' {
        Register-PSResourceRepository $TestRepoLocalName -URL $TestRepoLocalURL -Trusted

        $repo = Get-PSResourceRepository $TestRepoLocalName
        $repo.Name | should be $TestRepoLocalURL
        $repo.URL | should be $TestRepoLocalURL
        $repo.Trusted | should be $true
    }

    It 'Should register the test repository when -URL is a fileshare and priority is set' {
        Register-PSResourceRepository $TestRepoLocalName -URL $TestRepoLocalURL -Priority 2

        $repo = Get-PSResourceRepository $TestRepoLocalName
        $repo.Name | should be $TestRepoLocalURL
        $repo.URL | should be $TestRepoLocalURL
        $repo.Trusted | should be $true
        $repo.Priority | should be 2
    }

    It 'Should fail to reregister the repository when the -Name is already registered' {
        Register-PSResourceRepository $TestRepoLocalName -URL $TestRepoLocalURL
        Register-PSResourceRepository $TestRepoLocalName -URL $TestRepoLocalURL2 -ErrorVariable ev -ErrorAction SilentlyContinue

        $ev[0].FullyQualifiedErrorId | Should be "The PSResource Repository '$($TestRepoName)' exists."
    }

    It 'Should fail to reregister the repository when the fileshare -URL is already registered' {
        Register-PSResourceRepository $TestRepoLocalName -URL $TestRepoLocalURL
        Register-PSResourceRepository 'NewTestName' -URL $TestRepoLocalURL2 -ErrorVariable ev -ErrorAction SilentlyContinue

        $ev[0].FullyQualifiedErrorId | Should be "The repository could not be registered because there exists a registered repository with Name '$($TestRepoName)' and URL '$($TestRepoURL)'. To register another repository with Name '$($TestRepoName2)', please unregister the existing repository using the Unregister-PSResourceRepository cmdlet."
    }

    It 'Register PSResourceRepository File system location with special chars' {
        $tmpdir = Join-Path -Path ([System.IO.Path]::GetTempPath()) -ChildPath 'ps repo testing [$!@^&test(;)]'
        if (-not (Test-Path -LiteralPath $tmpdir)) {
            New-Item -Path $tmpdir -ItemType Directory > $null
        }
        try {
            Register-PSResourceRepository -Name 'Test Repository' -SourceLocation $tmpdir
            try {
                $repo = Get-PSResourceRepository -Name 'Test Repository'
                $repo.Name | should be 'Test Repository'
                $repo.URL | should be $tmpdir
            }
            finally {
                Unregister-PSReResourcepository -Name 'Test Repository' -ErrorAction SilentlyContinue
            }
        }
        finally {
            Remove-Item -LiteralPath $tmpdir -Force -Recurse
        }
    }
}


Describe 'Registering Repositories with Hashtable Parameters' -tags 'BVT', 'InnerLoop' {

    AfterAll {
        Unregister-PSResourceRepository -Name $PSGalleryName -ErrorAction SilentlyContinue
        Unregister-PSResourceRepository -Name $TestRepoName -ErrorAction SilentlyContinue
        Unregister-PSResourceRepository -Name $TestRepoName2 -ErrorAction SilentlyContinue
        Unregister-PSResourceRepository -Name $TestRepoLocalName -ErrorAction SilentlyContinue
        Unregister-PSResourceRepository -Name $TestRepoLocalName2 -ErrorAction SilentlyContinue
    }

    BeforeEach {
        Unregister-PSResourceRepository -Name $PSGalleryName -ErrorAction SilentlyContinue
        Unregister-PSResourceRepository -Name $TestRepoName -ErrorAction SilentlyContinue
        Unregister-PSResourceRepository -Name $TestRepoName2 -ErrorAction SilentlyContinue
        Unregister-PSResourceRepository -Name $TestRepoLocalName -ErrorAction SilentlyContinue
        Unregister-PSResourceRepository -Name $TestRepoLocalName2 -ErrorAction SilentlyContinue
    }

    It 'Should register a repository with parameters as a hashtable' {
        $paramRegisterPSResourceRepository = @{
            Name     = $TestRepoName
            URL      = $TestRepoURL
            Trusted  = $false
            Priority = 1
        }

        { Register-PSResourceRepository @paramRegisterPSResourceRepository } | Should not Throw

        $repo = Get-PSResourceRepository -Name $TestRepoName
        $repo.URL | Should be $TestRepoURL
        $repo.Trusted | Should be $True
        $repo.Priority | Should be 1
    }

    It 'Should register multiple repositories' {
        Register-PSResourceRepository -Repositories @(
            @{ Name = $TestRepoName; URL = $TestRepoURL; Priority = 15 }
            @{ Name = $TestRepoLocalName; URL = $TestRepoLocalURL }
            @{ PSGallery = $true; Trusted = $true }
        )

        $repos = Get-PSResourceRepository
        $repos.Count | Should be 3
        $repo1 = Get-PSResourceRepository $TestRepoName
        $repo1.URL | Should be $TestRepoURL
        $repo1.Priority | Should be 15
        $repo2 = Get-PSResourceRepository $TestRepoLocalName
        $repo2.URL | Should be $TestRepoLocalURL
        $repo2.Priority | Should be 50
        $repo3 = Get-PSResourceRepository $PSGalleryName
        $repo3.URL | Should be $PSGalleryLocation
        $repo3.Priority | Should be 0
    }
}


################################
### Set-PSResourceRepository ###
################################
Describe 'Test Set-PSResourceRepository' -tags 'BVT', 'InnerLoop' {

    BeforeAll {
    }

    AfterAll {
    }

    BeforeEach {
        Register-PSResourceRepository -PSGallery -ErrorAction SilentlyContinue
    }

    It 'Should set PSGallery to a trusted installation policy' {
        Set-PSResourceRepository $PSGalleryName -Trusted

        $repo = Get-PSResourceRepository $PSGalleryName
        $repo.Trusted | should be $true
        $repo.Priority | should be 0
    }

    It 'Should set PSGallery to a trusted installation policy and a non-zero priority' {
        Set-PSResourceRepository $PSGalleryName -Trusted -Priority 3

        $repo = Get-PSResourceRepository $PSGalleryName
        $repo.Trusted | should be $true
        $repo.Priority | should be 3
    }

    It 'Should set PSGallery to an untrusted installation policy' {
        Set-PSResourceRepository -Name $PSGalleryName -Trusted
        Set-PSResourceRepository -Name $PSGalleryName -Trusted:$false

        $repo = Get-PSResourceRepository $PSGalleryName
        $repo.Trusted | should be $false
        $repo.Priority | should 50
    }

    It 'Should fail to set PSGallery to a different URL' {
        Set-PSResourceRepository $PSGalleryName -URL $TestRepoURL -ErrorVariable ev -ErrorAction SilentlyContinue

        $ev[0].FullyQualifiedErrorId | Should be "The PSGallery repository has pre-defined locations. Setting the 'URL' parameter is not allowed, try again after removing the 'URL' parameter."
    }
}

Describe 'Test Set-PSResourceRepository with hashtable parameters' -tags 'BVT', 'InnerLoop' {

    AfterAll {
    }
    BeforeAll {
    }

    BeforeEach {
        Unregister-PSResourceRepository -Name $PSGalleryName -ErrorAction SilentlyContinue
        Unregister-PSResourceRepository -Name $TestRepoName -ErrorAction SilentlyContinue
        Unregister-PSResourceRepository -Name $TestRepoName2 -ErrorAction SilentlyContinue
        Unregister-PSResourceRepository -Name $TestRepoLocalName -ErrorAction SilentlyContinue
        Unregister-PSResourceRepository -Name $TestRepoLocalName2 -ErrorAction SilentlyContinue

        Register-PSResourceRepository -PSGallery -Trusted
        Register-PSResourceRepository -Name $TestRepoName -URL $TestRepoURL -Trusted
        Register-PSResourceRepository -Name $TestRepoName2 -URL $TestRepoURL2 -Priority 15
        Register-PSResourceRepository -Name $TestRepoLocalName -URL $TestRepoLocalURL
        Register-PSResourceRepository -Name $TestRepoLocalName2 -URL $TestRepoLocalURL2
    }


    It 'Should set repository with given hashtable parameters' {
        $paramSetPSResourceRepository = @{
            Name     = $TestRepoName
            URL      = $TestRepoURL2
            Trusted  = $False
            Priority = 1
        }

        { Set-PSRepository @paramSetPSResourceRepository } | Should not Throw

        $repo = Get-PSRepository -Name $TestRepoName
        $repo.URL | Should be $TestRepoURL2
        $repo.Trusted | Should be $False
        $repo.Priority | Should be 1
    }

    It 'Should set multiple repositories' {
        $repositories = @{
            @{ Name = $TestRepoName2; URL = $TestRepoURL; Priority = 9 }
            @{ Name = $TestRepoLocalName; URL = $TestRepoLocalURL2; Trusted =$True }
            @{ Name = $PSGalleryName; Trusted = $False }
        }

        { Set-PSResourceRepository -Repositories $repositories } | Should not Throw

        $repos = Get-PSResourceRepository
        $repos.Count | Should be 3
        $repo1 = Get-PSResourceRepository $TestRepoName2
        $repo1.URL | Should be $TestRepoURL
        $repo.Trusted | Should be $False
        $repo1.Priority | Should be 9

        $repo2 = Get-PSResourceRepository $TestRepoLocalName
        $repo2.URL | Should be $TestRepoLocalURL2
        $repo.Trusted | Should be $True
        $repo2.Priority | Should be 0

        $repo3 = Get-PSResourceRepository $PSGalleryName
        $repo3.URL | Should be $PSGalleryLocation
        $repo.Trusted | Should be $False
        $repo3.Priority | Should be 50
    }

}



################################
### Get-PSResourceRepository ###
################################
Describe 'Test Get-PSResourceRepository' -tags 'BVT', 'InnerLoop' {

    BeforeAll {
    }

    AfterAll {
    }

    BeforeEach {
        Register-PSResourceRepository -PSGallery -Trusted -ErrorAction SilentlyContinue
        Register-PSResourceRepository -Name $TestRepoName -URL $TestRepoURL -Trusted -ErrorAction SilentlyContinue
        Register-PSResourceRepository -Name $TestRepoName2 -URL $TestRepoURL2 -Priority 15 -ErrorAction SilentlyContinue
        Register-PSResourceRepository -Name $TestRepoLocalName -URL $TestRepoLocalURL -ErrorAction SilentlyContinue
        Register-PSResourceRepository -Name $TestRepoLocalName2 -URL $TestRepoLocalURL2 -ErrorAction SilentlyContinue
    }

    It 'Should get PSGallery repository' {
        $repo = Get-PSResourceRepository $PSGalleryName
        $repo.URL | should be $PSGalleryLocation
        $repo.Trusted | should be $false
        $repo.Priority | should be 50
    }

    It 'Should get test repository' {
        $repo = Get-PSResourceRepository $TestRepoName
        $repo.URL | should be $TestRepoURL
        $repo.Trusted | should be $true
        $repo.Priority | should be 0
    }

    It 'Should get multiple repositories' {
        $repos = Get-PSResourceRepository $PSGalleryName, $TestRepoName, $TestRepoLocalName

        $repos.Count | Should be 3

        $PSGalleryName | should -BeIn $repos.Name
        $TestRepoName | should -BeIn $repos.Name
        $TestRepoLocalName | should -BeIn $repos.Name

        $PSGalleryLocation | should -BeIn $repos.URL
        $TestRepoURL | should -BeIn $repos.URL
        $TestRepoLocalURL | should -BeIn $repos.URL

        0 | should -BeIn $repos.Priority
        50 | should -BeIn $repos.Priority
    }

    It 'Should get all repositories' {
        $repos = Get-PSResourceRepository

        $repos.Count | Should be 5

        $PSGalleryName | should -BeIn $repos.Name
        $TestRepoName | should -BeIn $repos.Name
        $TestRepoName2 | should -BeIn $repos.Name
        $TestRepoLocalName | should -BeIn $repos.Name
        $TestRepoLocalName2 | should -BeIn $repos.Name

        $PSGalleryLocation | should -BeIn $repos.URL
        $TestRepoURL | should -BeIn $repos.URL
        $TestRepoURL2 | should -BeIn $repos.URL
        $TestRepoLocalURL | should -BeIn $repos.URL
        $TestRepoLocalURL2 | should -BeIn $repos.URL

        0 | should -BeIn $repos.Priority
        50 | should -BeIn $repos.Priority
        15 | should -BeIn $repos.Priority
    }
}



#######################################
### Unregister-PSResourceRepository ###
#######################################

Describe 'Test Unregister-PSResourceRepository' -tags 'BVT' {

    BeforeAll {

    }
    AfterAll {
        Unregister-PSResourceRepository -Name $PSGalleryName -ErrorAction SilentlyContinue
        Unregister-PSResourceRepository -Name $TestRepoName -ErrorAction SilentlyContinue
        Unregister-PSResourceRepository -Name $TestRepoName2 -ErrorAction SilentlyContinue
        Unregister-PSResourceRepository -Name $TestRepoLocalName -ErrorAction SilentlyContinue
        Unregister-PSResourceRepository -Name $TestRepoLocalName2 -ErrorAction SilentlyContinue
    }

    BeforeEach {
       Register-PSResourceRepository -PSGallery -Trusted -ErrorAction SilentlyContinue
       Register-PSResourceRepository -Name $TestRepoName -URL $TestRepoURL -Trusted -ErrorAction SilentlyContinue
       Register-PSResourceRepository -Name $TestRepoName2 -URL $TestRepoURL2 -Priority 15 -ErrorAction SilentlyContinue
       Register-PSResourceRepository -Name $TestRepoLocalName -URL $TestRepoLocalURL -ErrorAction SilentlyContinue
       Register-PSResourceRepository -Name $TestRepoLocalName2 -URL $TestRepoLocalURL2 -ErrorAction SilentlyContinue
    }

    ### Unregistering the PowerShell Gallery
    It 'Should unregister the default PSGallery' {
        Unregister-PSResourceRepository $PSGalleryName

        $repo = Get-PSResourceRepository $PSGalleryName -ErrorVariable ev -ErrorAction SilentlyContinue
        $repo | Should -BeNullOrEmpty
        $ev[0].FullyQualifiedErrorId | Should be "Unable to find repository 'PSGallery'. Use Get-PSResourceRepository to see all available repositories."
    }

    It 'Should unregister multiple repositories' {
        Unregister-PSResourceRepository $TestRepoName, $TestRepoName2, $TestRepoLocalName

        $repos = Get-PSResourceRepository $TestRepoName, $TestRepoName2, $TestRepoLocalName -ErrorVariable ev -ErrorAction SilentlyContinue
        $repos | Should -BeNullOrEmpty
        $ev[0].FullyQualifiedErrorId | Should be "Unable to find repository 'PSGallery'. Use Get-PSResourceRepository to see all available repositories."

    }
}
