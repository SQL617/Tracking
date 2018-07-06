function [ fileNameColVec ] = fileRename(directory,newFolderName,rule)

directory = char(directory);
newFolderName = char(newFolderName);
rule = char(rule)';
% Copy and rename files 
if ~exist('newFolderName','var') || isempty(newFolderName)
  newFolderName='simple';
end

% First create the folder B, if necessary.
cd(directory);
outputFolder = fullfile(pwd, newFolderName);
if ~exist(outputFolder, 'dir')
    cd(directory)
	mkdir(outputFolder);
end
% Copy the files over with a new name.
cd(directory);
inputFiles = dir( fullfile('*.jpg') );
fileNames = { inputFiles.name };
% Create column vector to store file names.
fileNameColVec = (strings([(size(fileNames,2)),1]));
    for k = 1 : length(inputFiles )
        thisFileName = fileNames{k};
        % Prepare the input filename.
        inputFullFileName = fullfile(pwd, thisFileName)
        % Prepare the output filename.    
        if (rule == '_')
            thisFileNameBasic = extractBefore(thisFileName,'_');
        elseif (rule == ')')
            thisFileNameBasic = extractAfter(thisFileName,')');
            thisFileNameBasic = thisFileNameBasic(1:end-3);
        end
        thisFileName = sprintf( '%06d', str2double(thisFileNameBasic) ) ;
        % Add filename to column vector.
        fileNameColVec(k) = num2str(thisFileName);
        outputBaseFileName = sprintf('%s.jpg', thisFileName);
        outputFullFileName = fullfile(outputFolder, outputBaseFileName)
        % Do the copying and renaming all at once.
        copyfile(inputFullFileName, outputFullFileName);
        s = dir(outputBaseFileName);    
        
    end
    % Delete old files from parent folder
    cd(directory);
    delete *.jpg;
    cd ..\;
end