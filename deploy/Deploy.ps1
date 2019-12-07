
param ([string] $artifactsBucket)

aws cloudformation package `
    --template-file $PSScriptRoot/CustomAuthorizer.yaml `
    --s3-bucket $artifactsBucket `
    --s3-prefix CustomAuthorizer `
    --output-template-file $PSScriptRoot/packaged-CustomAuthorizer.yaml

aws cloudformation deploy `
    --template-file "$PSScriptRoot/packaged-CustomAuthorizer.yaml" `
    --stack-name CustomAuthorizer `
    --capabilities CAPABILITY_NAMED_IAM