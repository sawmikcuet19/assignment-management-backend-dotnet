namespace AssignmentManagement.Api.Domain;

public enum AssignmentStatus
{
    Draft = 0,
    Published = 1,
    Archived = 2
}

public enum SubmissionStatus
{
    Submitted = 0,
    UnderReview = 1,
    Graded = 2,
    Returned = 3
}
